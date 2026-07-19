import { CommonModule } from '@angular/common';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { firstValueFrom } from 'rxjs';
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
}