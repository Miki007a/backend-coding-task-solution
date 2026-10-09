# Insurance claims API

This API keeps marine insurance policies and the damage claims filed against them.

A **cover** is a policy on one ship for a date range. The ship is a yacht, passenger ship, container ship, bulk carrier, or tanker. The price of the policy is the premium, and the API calculates it. The client does not send the premium or the id.

A **claim** is a damage report against one existing cover. It has a name, a type (collision, grounding, bad weather, or fire), a damage cost, and the day it was created. That day has to fall inside the cover's date range.

Claims and covers are stored in MongoDB. Creating or deleting either one also records an audit entry in SQL Server.

## Run locally

Docker Desktop has to be running. Starting the API starts a SQL Server container and a MongoDB container.

```powershell
dotnet run --project Claims
```

Swagger is at <https://localhost:7052/swagger>. The API also listens on <http://localhost:5052>.

```powershell
dotnet test
```

## Premium

The premium is the fee for the whole cover. It depends on the ship type and how many days the policy lasts.

The length is the number of calendar days from the start date up to, but not including, the end date. A cover from 1 January to 15 February is 45 days. A cover that starts and ends on the same day has length 0 and is rejected.

One day starts from a base rate of 1250:

| Ship | Day rate |
|---|---|
| Yacht | 1375 (10% more) |
| Passenger ship | 1500 (20% more) |
| Tanker | 1875 (50% more) |
| Container ship, bulk carrier, and any other type | 1625 (30% more) |

Longer policies get a lower rate on later days. The discount applies only to that band of days.

- The first 30 days use the full day rate.
- The next 150 days are 5% off for a yacht and 2% off for every other ship.
- Every day after that is 8% off for a yacht and 3% off for every other ship. That is the earlier discount plus a further 3% for a yacht and a further 1% for the others. The percentages are added, not multiplied.

A 45-day yacht is 30 days at 1375 plus 15 days at 5% off 1375.

## What the API accepts

Dates are calendar dates. "Today" is the current UTC date.

A cover is saved only when:

- the start date is today or later
- the end date is after the start date
- the end date is no later than one year after the start date, so a cover from 1 January 2026 through 1 January 2027 is allowed

A claim is saved only when:

- the damage cost is 100000 or less
- the cover id exists
- the created date is on or between that cover's start and end dates

A broken rule returns 400 and a body that lists the fields. A missing claim, a missing cover, or a claim for a cover that does not exist returns 404. Deleting something that is already gone does not write an audit.

## Auditing

A successful create or delete records which id changed, whether the call was `POST` or `DELETE`, and the UTC time. Reads are not audited.

The HTTP request does not wait for that row to be written. It places the audit on an in-memory queue of 100 items and returns. A background worker reads the queue and inserts the row into SQL Server. If the queue is full, the request waits until there is room. If one insert fails, the failure is logged and the worker reads the next item.

The queue lives in the process. An audit that has not been written yet is lost when the process stops or crashes.

## Where the code lives

| Folder | What is there |
|---|---|
| `Claims/Controllers` | HTTP: read the request, call a service, return the status code. |
| `Claims/Contracts` | The create-claim and create-cover request bodies. |
| `Claims/Services` | Creating, reading, and deleting claims and covers, the premium calculation, and the date rules. |
| `Claims/Domain` | The claim and cover models. |
| `Claims/Infrastructure/Persistence` | MongoDB. |
| `Claims/Infrastructure/Auditing` | The queue, the worker, and the SQL Server audit tables. |

`ClaimService` asks `IClaimRepository` and `IAuditer` for storage and auditing. It does not talk to MongoDB or SQL Server itself. The application connects those interfaces to the MongoDB repository and the queue when it starts.
