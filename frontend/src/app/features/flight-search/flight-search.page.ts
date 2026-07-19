import { Component, computed, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { FlightCardComponent } from '../../shared/components/flight-card/flight-card.component';
import { EmptyStateComponent } from '../../shared/components/empty-state/empty-state.component';
import { Flight } from '../../core/models/flight.model';

@Component({
  selector: 'app-flight-search-page',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, FlightCardComponent, EmptyStateComponent],
  templateUrl: './flight-search.page.html',
  styleUrl: './flight-search.page.scss'
})
export class FlightSearchPage {
  private readonly fb = new FormBuilder();

  readonly flights = signal<Flight[]>([
    {
      id: 'FL-001',
      provider: 'GlobalAir',
      from: 'JFK',
      to: 'LAX',
      departureTime: '08:40',
      arrivalTime: '11:20',
      durationMinutes: 340,
      price: 280,
      cabinClass: 'Economy'
    },
    {
      id: 'FL-002',
      provider: 'BudgetWings',
      from: 'JFK',
      to: 'LAX',
      departureTime: '13:10',
      arrivalTime: '16:25',
      durationMinutes: 335,
      price: 215,
      cabinClass: 'Economy'
    }
  ]);

  readonly filters = this.fb.group({
    origin: ['', Validators.required],
    destination: ['', Validators.required],
    departureDate: ['', Validators.required],
    passengers: [1, [Validators.required, Validators.min(1)]]
  });

  readonly hasFlights = computed(() => this.flights().length > 0);

  search(): void {
    this.filters.markAllAsTouched();
  }
}