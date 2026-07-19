import { CommonModule } from '@angular/common';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { BookingSelection } from '../../core/models/booking-selection.model';
import { BookingSelectionService } from '../../core/services/booking-selection.service';
import { FlightApiService, FlightSearchResult } from '../../core/services/flight-api.service';
import { Airport } from '../../core/models/airport.model';

type FlightSortOrder = 'priceAsc' | 'priceDesc' | 'durationAsc' | 'departureAsc';

@Component({
  selector: 'app-flight-search-page',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './flight-search.page.html',
  styleUrl: './flight-search.page.scss'
})
export class FlightSearchPage {
  private readonly fb = inject(FormBuilder).nonNullable;
  private readonly flightApiService = inject(FlightApiService);
  private readonly bookingSelectionService = inject(BookingSelectionService);
  private readonly router = inject(Router);

  readonly airports = signal<Airport[]>([]);
  readonly sortOrder = signal<FlightSortOrder>('priceAsc');

  readonly sortOptions: Array<{ value: FlightSortOrder; label: string }> = [
    { value: 'priceAsc', label: 'Price: Low to High' },
    { value: 'priceDesc', label: 'Price: High to Low' },
    { value: 'durationAsc', label: 'Duration: Shortest First' },
    { value: 'departureAsc', label: 'Departure Time: Earliest First' }
  ];

  readonly cabinClasses = ['Economy', 'Business', 'First Class'] as const;

  readonly flights = signal<FlightSearchResult[]>([]);
  readonly loading = signal(false);
  readonly errorMessage = signal<string | null>(null);

  readonly filters = this.fb.group({
    origin: ['', Validators.required],
    destination: ['', Validators.required],
    departureDate: ['', Validators.required],
    passengers: [1, [Validators.required, Validators.min(1), Validators.max(9)]],
    cabinClass: ['Economy', Validators.required]
  });

  readonly hasFlights = computed(() => this.flights().length > 0);
  readonly sortedFlights = computed(() => this.sortFlights(this.flights(), this.sortOrder()));
  readonly hasSearched = signal(false);

  constructor() {
    this.filters.addValidators(() => {
      const origin = this.filters.controls.origin.value;
      const destination = this.filters.controls.destination.value;

      return origin && destination && origin === destination ? { sameAirport: true } : null;
    });
  }

  onSortOrderChange(order: FlightSortOrder): void {
    this.sortOrder.set(order);
  }

  formatAirportLabel(airport: Airport): string {
    return this.formatAirportLabelParts(airport.name, airport.country, airport.code);
  }

  selectFlight(flight: FlightSearchResult): void {
    const formValue = this.filters.getRawValue();
    const originAirport = this.airports().find((airport) => airport.code === formValue.origin);
    const destinationAirport = this.airports().find((airport) => airport.code === formValue.destination);

    if (!originAirport || !destinationAirport) {
      return;
    }

    const selection: BookingSelection = {
      flightId: flight.flightId,
      providerName: flight.providerName,
      originCode: flight.originCode,
      originName: originAirport.name,
      originCountry: originAirport.country,
      originCountryCode: originAirport.countryCode,
      destinationCode: flight.destinationCode,
      destinationName: destinationAirport.name,
      destinationCountry: destinationAirport.country,
      destinationCountryCode: destinationAirport.countryCode,
      departureTimeUtc: flight.departureTimeUtc,
      arrivalTimeUtc: flight.arrivalTimeUtc,
      cabinClass: flight.cabinClass,
      pricePerPassenger: flight.pricePerPassenger,
      totalPrice: flight.totalPrice,
      passengerCount: formValue.passengers
    };

    this.bookingSelectionService.setSelection(selection);
    void this.router.navigate(['/booking']);
  }

  async ngOnInit(): Promise<void> {
    const airports = await firstValueFrom(this.flightApiService.getAirports());
    this.airports.set(airports);
  }

  async search(): Promise<void> {
    this.filters.markAllAsTouched();

    if (this.filters.invalid) {
      return;
    }

    const formValue = this.filters.getRawValue();

    this.loading.set(true);
    this.errorMessage.set(null);
    this.hasSearched.set(true);

    try {
      const response = await firstValueFrom(
        this.flightApiService.search({
          originCode: formValue.origin,
          destinationCode: formValue.destination,
          departureDate: formValue.departureDate,
          numberOfPassengers: formValue.passengers,
          cabinClass: formValue.cabinClass
        })
      );

      this.flights.set(response.flights);
    } catch {
      this.flights.set([]);
      this.errorMessage.set('Unable to load flights. Please try again.');
    } finally {
      this.loading.set(false);
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

  formatDuration(departureTimeUtc: string, arrivalTimeUtc: string): string {
    const departureTime = new Date(departureTimeUtc).getTime();
    const arrivalTime = new Date(arrivalTimeUtc).getTime();
    const durationMinutes = Math.max(0, Math.round((arrivalTime - departureTime) / 60000));
    const hours = Math.floor(durationMinutes / 60);
    const minutes = durationMinutes % 60;

    return `${hours}h ${minutes.toString().padStart(2, '0')}m`;
  }

  private sortFlights(flights: FlightSearchResult[], order: FlightSortOrder): FlightSearchResult[] {
    return [...flights].sort((left, right) => {
      switch (order) {
        case 'priceAsc':
          return left.totalPrice - right.totalPrice;
        case 'priceDesc':
          return right.totalPrice - left.totalPrice;
        case 'durationAsc': {
          const leftDuration = new Date(left.arrivalTimeUtc).getTime() - new Date(left.departureTimeUtc).getTime();
          const rightDuration = new Date(right.arrivalTimeUtc).getTime() - new Date(right.departureTimeUtc).getTime();

          return leftDuration - rightDuration;
        }
        case 'departureAsc':
          return new Date(left.departureTimeUtc).getTime() - new Date(right.departureTimeUtc).getTime();
      }
    });
  }

  private formatAirportLabelParts(name: string, country: string, code: string): string {
    const normalizedName = name.replace(new RegExp(`\s*\(\s*${code}\s*\)\s*$`, 'i'), '').trim();

    if (!country) {
      return `${normalizedName} (${code})`;
    }

    return `${normalizedName}, ${country} (${code})`;
  }
}