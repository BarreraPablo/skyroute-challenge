import { Component, input } from '@angular/core';
import { Flight } from '../../../core/models/flight.model';

@Component({
  selector: 'app-flight-card',
  standalone: true,
  templateUrl: './flight-card.component.html',
  styleUrl: './flight-card.component.scss'
})
export class FlightCardComponent {
  readonly flight = input.required<Flight>();
}