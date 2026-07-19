import { Injectable } from '@angular/core';
import { Flight } from '../models/flight.model';

@Injectable({ providedIn: 'root' })
export class FlightApiService {
  search(): Flight[] {
    return [];
  }
}