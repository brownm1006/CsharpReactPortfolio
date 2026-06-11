# API Contract

This folder contains the contract-first API definition for the C# backend.

## Files

- `quote-api.openapi.json`: OpenAPI 3.1 document shaped like the JSON document produced by ASP.NET Core built-in OpenAPI support.

## Backend Implementation Map

The first backend implementation should focus on:

- `POST /api/quotes`
  - Inserts into `quote.quotes`.
- `GET /api/quotes/{quoteId}/vehicles`
  - Reads from `quote.quote_vehicles` and `quote.quote_vehicle_usages`.
- `POST /api/quotes/{quoteId}/vehicles`
  - Inserts into `quote.quote_vehicles`.
  - Inserts matching usage data into `quote.quote_vehicle_usages`.
  - Resolves lookup codes against `quote.vehicle_manufacturers`, `quote.vehicle_models`, `quote.yes_no_unknown_options`, and `quote.purchase_condition_options`.
- `DELETE /api/quotes/{quoteId}/vehicles/{vehicleId}`
  - Deletes from `quote.quote_vehicles`; usage rows cascade through the schema.
- `GET /api/quote/lookups/*`
  - Reads active select-box values from the lookup tables.

The React frontend currently needs the vehicle description lookups, vehicle usage lookups, quote creation, vehicle creation, vehicle listing, and vehicle deletion endpoints.
