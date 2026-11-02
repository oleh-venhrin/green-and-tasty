# Green & Tasty — Restaurant Reservation & Group Pre-Order Service

Web application developed and deployed for the EPAM IT Marathon (Fall 2026).  
The project implements a full lifecycle service for restaurant table booking and collaborative group pre-ordering (**Direction 1 · Group Pre-Order**).

---

## 🚀 Live Environment

- **Frontend (Web Application):** [https://dd55jazix0ytj.cloudfront.net](https://dd55jazix0ytj.cloudfront.net)
- **Backend API:** [https://7buunq3yi4.execute-api.us-west-2.amazonaws.com/develop](https://7buunq3yi4.execute-api.us-west-2.amazonaws.com/develop)
- **AWS Region:** `us-west-2`

---

## 🛠️ Scope of Completed Work

### 1. Cloud Infrastructure & DevOps (AWS & Terraform)
- **Hosting & CDN:** Deployed Amazon S3 bucket integrated with Amazon CloudFront distribution for fast global delivery and SSL/HTTPS termination.
- **Compute & API:** Configured AWS Lambda functions (.NET 8 runtime) behind Amazon API Gateway with custom authorizers and CORS.
- **Database:** Amazon DynamoDB tables (`Reservations`, `Orders`, `GroupReservations`, `GuestSessions`, `Dishes`, `Users`, `Tables`, `Locations`).
- **Identity & Security:** Amazon Cognito User Pool for customer and waiter authentication.
- **CI/CD:** Automated GitHub Actions workflows for continuous deployment and testing.

### 2. Backend Development (.NET 8 Serverless)
- Implemented core business logic for **Group Pre-Order**:
  - Generation and verification of secure group invite tokens.
  - Creation and management of guest sessions (`GuestSessions`).
  - Retrieval and aggregation of dishes and totals across participants.
  - Guest cart submission and guest removal by the reservation owner.
  - Final consolidated pre-order submission to restaurant kitchen.
- Fixed API Gateway proxy route integrations and resolved null-safety issues on dish popularity sorting.

### 3. Frontend Development (Angular 18+)
- Built responsive user interface with Angular Signals and standalone components.
- Implemented **Group Pre-Order Dashboard** (`/reservations/:id/group-order`):
  - Real-time periodic polling synchronization (15s interval).
  - Consolidated order preview with dynamic subtotal and grand total calculation.
  - Individual participant cards displaying selection status (`Still choosing` / `Submitted`).
  - Modal confirmations for order submission and guest removal.

### 4. Automated Quality Assurance (AQA & Test Automation)
- Built an end-to-end BDD testing framework using **Reqnroll (.NET 10)**, **TUnit**, and **Playwright**:
  - Implemented the **Screenplay Pattern** (Actors, Tasks, Questions, Abilities) for maintainable tests.
  - Full automated coverage for Direction 1: API and UI smoke tests, security checks (owner validation), and negative scenarios.
- **CI/CD Quality Gates:**
  - `AQA Quality`: Static analysis, formatting, and Gherkin linting (0 warnings).
  - `AQA Tests`: 10/10 automated scenarios passing against the live AWS deployment.

---

## 📋 Technology Stack

- **Frontend:** Angular 18+, TypeScript, SCSS, RxJS, Signals
- **Backend:** .NET 8, C#, AWS Lambda, Amazon API Gateway, AWS Syndicate
- **Database & Storage:** Amazon DynamoDB, Amazon S3
- **Infrastructure as Code:** Terraform, AWS CLI
- **Testing:** Playwright, Reqnroll / SpecFlow, TUnit, FluentAssertions
- **CI/CD:** GitHub Actions
