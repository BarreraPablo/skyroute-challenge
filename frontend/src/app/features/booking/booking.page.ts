import { CommonModule } from '@angular/common';
import { Component, computed, effect, inject, signal } from '@angular/core';
import { AbstractControl, FormBuilder, ReactiveFormsModule, ValidationErrors, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { BookingApiService, BookingValidationResult } from '../../core/services/booking-api.service';
import { BookingSelectionService } from '../../core/services/booking-selection.service';

@Component({
  selector: 'app-booking-page',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './booking.page.html',
  styleUrl: './booking.page.scss'
})
export class BookingPage {
  private readonly fb = inject(FormBuilder).nonNullable;
  private readonly bookingSelectionService = inject(BookingSelectionService);
  private readonly bookingApiService = inject(BookingApiService);
  private readonly router = inject(Router);

  readonly selectedFlight = this.bookingSelectionService.selection;
  readonly isInternational = computed(() => {
    const selection = this.selectedFlight();

    return selection
      ? selection.originCountryCode.toUpperCase() !== selection.destinationCountryCode.toUpperCase()
      : false;
  });
  readonly documentLabel = computed(() => (this.isInternational() ? 'Passport Number' : 'National ID'));
  readonly pricePerPassenger = computed(() => this.selectedFlight()?.pricePerPassenger ?? 0);
  readonly passengerCount = computed(() => this.selectedFlight()?.passengerCount ?? 0);
  readonly totalPrice = computed(() => this.selectedFlight()?.totalPrice ?? 0);
  readonly documentGuidance = computed(() =>
    this.isInternational()
      ? 'Use a passport number with 6-9 alphanumeric characters.'
      : 'Use a national ID with 6-16 digits.'
  );

  readonly bookingForm = this.fb.group({
    fullName: ['', [Validators.required, Validators.minLength(3)]],
    email: ['', [Validators.required, Validators.email]],
    documentNumber: ['', Validators.required]
  });

  readonly bookingReference = signal<string | null>(null);
  readonly errorMessage = signal<string | null>(null);
  readonly submitting = signal(false);
  readonly bookingCompleted = signal(false);

  constructor() {
    effect(() => {
      const selection = this.selectedFlight();
      const documentNumberControl = this.bookingForm.controls.documentNumber;

      if (!selection) {
        documentNumberControl.setValidators([Validators.required]);
        documentNumberControl.updateValueAndValidity({ emitEvent: false });
        return;
      }

      documentNumberControl.setValidators([
        Validators.required,
        this.documentNumberValidator(selection.originCountryCode !== selection.destinationCountryCode)
      ]);
      documentNumberControl.updateValueAndValidity({ emitEvent: false });
    });
  }

  async submit(): Promise<void> {
    if (this.submitting() || this.bookingCompleted()) {
      return;
    }

    this.bookingForm.markAllAsTouched();

    const selection = this.selectedFlight();

    if (!selection) {
      this.errorMessage.set('Select a flight from search results before booking.');
      return;
    }

    if (this.bookingForm.invalid) {
      return;
    }

    const formValue = this.bookingForm.getRawValue();

    this.submitting.set(true);
    this.errorMessage.set(null);
    this.bookingReference.set(null);

    try {
      const response = await firstValueFrom(
        this.bookingApiService.create({
          flightId: selection.flightId,
          provider: selection.providerName,
          originCode: selection.originCode,
          destinationCode: selection.destinationCode,
          departureTimeUtc: selection.departureTimeUtc,
          arrivalTimeUtc: selection.arrivalTimeUtc,
          cabinClass: selection.cabinClass,
          pricePerPassenger: selection.pricePerPassenger,
          passengerCount: selection.passengerCount,
          expectedPrice: selection.totalPrice,
          passenger: {
            fullName: formValue.fullName.trim(),
            email: formValue.email.trim(),
            documentNumber: formValue.documentNumber.trim()
          }
        })
      );

      this.bookingReference.set(response.bookingReference);
      this.bookingCompleted.set(true);
      this.bookingForm.disable({ emitEvent: false });
    } catch (error) {
      this.errorMessage.set(this.extractErrorMessage(error));
    } finally {
      this.submitting.set(false);
    }
  }

  formatDateTime(value: string): string {
    return new Intl.DateTimeFormat('en-US', {
      dateStyle: 'medium',
      timeStyle: 'short',
      timeZone: 'UTC'
    }).format(new Date(value));
  }

  formatCurrency(value: number): string {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'USD',
      currencyDisplay: 'code',
      minimumFractionDigits: 2,
      maximumFractionDigits: 2
    }).format(value);
  }

  fullNameError(): string | null {
    const control = this.bookingForm.controls.fullName;

    if (!control.touched) {
      return null;
    }

    if (control.hasError('required')) {
      return 'Full name is required.';
    }

    if (control.hasError('minlength')) {
      return 'Full name must be at least 3 characters long.';
    }

    return null;
  }

  emailError(): string | null {
    const control = this.bookingForm.controls.email;

    if (!control.touched) {
      return null;
    }

    if (control.hasError('required')) {
      return 'Email address is required.';
    }

    if (control.hasError('email')) {
      return 'Enter a valid email address.';
    }

    return null;
  }

  documentNumberError(): string | null {
    const control = this.bookingForm.controls.documentNumber;

    if (!control.touched) {
      return null;
    }

    if (control.hasError('required')) {
      return `${this.documentLabel()} is required.`;
    }

    if (control.hasError('passportFormat')) {
      return 'Passport Number must be 6-9 alphanumeric characters.';
    }

    if (control.hasError('nationalIdFormat')) {
      return 'National ID must be 6-16 digits.';
    }

    return null;
  }

  hasSelection(): boolean {
    return this.selectedFlight() !== null;
  }

  get canSubmit(): boolean {
    return !this.submitting() && !this.bookingCompleted();
  }

  formatAirportLabel(name: string, country: string, code: string): string {
    const normalizedName = name.replace(new RegExp(`\s*\(\s*${code}\s*\)\s*$`, 'i'), '').trim();

    if (!country) {
      return `${normalizedName} (${code})`;
    }

    return `${normalizedName}, ${country} (${code})`;
  }

  async returnToSearch(): Promise<void> {
    this.bookingSelectionService.clearSelection();
    await this.router.navigate(['/']);
  }

  private documentNumberValidator(isInternational: boolean) {
    return (control: AbstractControl<string>): ValidationErrors | null => {
      const value = control.value.trim();

      if (!value) {
        return null;
      }

      if (isInternational) {
        return /^[A-Za-z0-9]{6,9}$/.test(value) ? null : { passportFormat: true };
      }

      return /^\d{6,16}$/.test(value) ? null : { nationalIdFormat: true };
    };
  }

  private extractErrorMessage(error: unknown): string {
    if (this.isBookingValidationResult(error)) {
      const messages = error.error.conditions
        .filter((condition) => condition.severity === 'Error')
        .map((condition) => condition.message);

      if (messages.length > 0) {
        return messages.join(' ');
      }
    }

    return 'Unable to confirm booking. Please review the details and try again.';
  }

  private isBookingValidationResult(error: unknown): error is { error: BookingValidationResult } {
    return Boolean(
      error
      && typeof error === 'object'
      && 'error' in error
      && typeof (error as { error?: unknown }).error === 'object'
      && (error as { error?: BookingValidationResult }).error !== null
      && 'conditions' in ((error as { error?: BookingValidationResult }).error ?? {})
    );
  }
}