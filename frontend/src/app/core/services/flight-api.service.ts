import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface FlightSearchRequest {
  originCode: string;
  destinationCode: string;
  departureDate: string;
  numberOfPassengers: number;
  cabinClass: string;
}

export interface FlightSearchResult {
  providerName: string;
  flightId: string;
  originCode: string;
  destinationCode: string;
  departureTimeUtc: string;
  arrivalTimeUtc: string;
  cabinClass: string;
  pricePerPassenger: number;
  totalPrice: number;
}

export interface FlightSearchResponse {
  flights: FlightSearchResult[];
}

export interface AirportResponse {
  code: string;
  name: string;
  country: string;
  countryCode: string;
}

@Injectable({ providedIn: 'root' })
export class FlightApiService {
  private readonly http = inject(HttpClient);

  search(request: FlightSearchRequest): Observable<FlightSearchResponse> {
    const params = new HttpParams({
      fromObject: {
        originCode: request.originCode,
        destinationCode: request.destinationCode,
        departureDate: request.departureDate,
        numberOfPassengers: request.numberOfPassengers.toString(),
        cabinClass: request.cabinClass
      }
    });

    return this.http.get<FlightSearchResponse>(`${environment.apiBaseUrl}/flights/search`, { params });
  }

  getAirports(): Observable<AirportResponse[]> {
    return this.http.get<AirportResponse[]>(`${environment.apiBaseUrl}/airports`);
  }
}