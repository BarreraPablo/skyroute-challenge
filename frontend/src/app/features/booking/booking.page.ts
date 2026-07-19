import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';

@Component({
  selector: 'app-booking-page',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './booking.page.html',
  styleUrl: './booking.page.scss'
})
export class BookingPage {
  private readonly fb = new FormBuilder();

  readonly bookingForm = this.fb.group({
    firstName: ['', Validators.required],
    lastName: ['', Validators.required],
    documentType: ['Passport', Validators.required],
    documentNumber: ['', Validators.required]
  });

  submit(): void {
    this.bookingForm.markAllAsTouched();
  }
}