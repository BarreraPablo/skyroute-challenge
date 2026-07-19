export interface Booking {
  flightId: string;
  passengerFirstName: string;
  passengerLastName: string;
  documentType: 'Passport' | 'NationalId';
  documentNumber: string;
}