export interface BookingSelection {
  flightId: string;
  providerName: string;
  originCode: string;
  originName: string;
  originCountry: string;
  originCountryCode: string;
  destinationCode: string;
  destinationName: string;
  destinationCountry: string;
  destinationCountryCode: string;
  departureTimeUtc: string;
  arrivalTimeUtc: string;
  cabinClass: string;
  pricePerPassenger: number;
  totalPrice: number;
  passengerCount: number;
}