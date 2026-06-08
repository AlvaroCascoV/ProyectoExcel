---
name: Student device-based check-in
overview: Add a student “check-in” flow using a stable DeviceIdentifier (GUID cookie) with IP as fallback/audit, plus an admin UI to map Device → Position and Position → Student (mock positions now, DB-backed later).
todos:
  - id: device-model
    content: Add Device + assignment/history entities and migrations (DeviceIdentifier + observed IP)
    status: pending
  - id: position-model
    content: Add mock Position table now (seeded) and position↔user assignment history (replaceable later)
    status: pending
  - id: device-registration
    content: Implement automatic device registration and cookie issuance (GUID) with IP audit capture
    status: pending
  - id: api-checkin-endpoint
    content: Implement POST /api/checkins using DeviceIdentifier (fallback to IP) and enforcing device/position ownership
    status: pending
  - id: admin-device-ui
    content: Add MVC admin screens to view devices, assign Device→Position and Position→Student, and show assignment history
    status: pending
  - id: student-checkin-ui
    content: Add MVC student check-in page that calls the API and shows device/position context
    status: pending
isProject: false
---

## Goal
Enable students to check in when they arrive and start a classroom Windows PC, associating the **current machine** to a **position/device number** (e.g. `W19`) and then to the correct student, recording a timestamped check-in.

## Constraints / reality check
- In a **browser**, you generally cannot read the Windows hostname (e.g. `T38W19`) reliably.
- You can capture the client IP as seen by the server (e.g. `HttpContext.Connection.RemoteIpAddress`), but it may change (DHCP) and may be masked by proxies/NAT.\n+- Therefore, the plan uses a **stable DeviceIdentifier** (GUID stored in a cookie) as the primary device key, with **IP stored for auditing** and a **fallback matching heuristic** only when the cookie is missing.

## Proposed UX (MVC)
- **Student**: a simple `CheckIn` page/button.\n+  - Student does *not* need to type `W19` in the normal flow.\n+  - The app identifies the **device** using the cookie and the admin-configured mapping to a **position**.\n+  - Page displays: detected position (e.g. `T38/W19`) + last seen IP (debug) + check-in confirmation.\n+- **Teacher/Admin**: a device management area.\n+  - See “unknown/new devices” as they appear.\n+  - Assign each device to a Position (e.g. `T38/W19`).\n+  - Assign Positions to students (mock now; designed to become production table later).\n+  - View assignment history (who was assigned when).

## Backend design (API)
### Device registration (automatic)
- For authenticated MVC/API usage, issue/read a cookie like `DeviceId` (GUID).\n+- API maintains a `Device` row keyed by `DeviceIdentifier` and updates `LastSeenAtUtc` and `LastSeenIp`.\n+- If cookie missing: create a new device + return a response that causes MVC to set the cookie.\n+\n+### Check-in endpoint\n+- Add `POST /api/checkins`:\n+  - Uses JWT claims (`tajamar_user_id`).\n+  - Resolves `Device` via `DeviceIdentifier` cookie; **fallback**: try to match a recent device by observed IP if cookie missing.\n+  - Determines the current `Position` from admin configuration (Device→Position).\n+  - Validates the logged-in student is the student assigned to that Position (Position→Student).\n+  - Writes a `CheckInRecord` (student, position, device, timestamp, observed IP).

## Data model (replaceable later, but DB-backed now)
### Device
- `Device`: stable row per machine/browser\n+  - `DeviceIdentifier` (GUID string)\n+  - `FirstSeenAtUtc`, `LastSeenAtUtc`\n+  - `LastSeenIp` (and optionally `LastSeenUserAgent`)\n+  - Optional: `FriendlyName`, `IsActive`\n+\n+### Positions (mock today, production-ready shape)
- `Position`: mocked/seeded positions like `T38/W19`.\n+  - `ClassCode` (e.g. `T38`)\n+  - `DeviceCode` (e.g. `W19`)\n+\n+### Assignments (history)
- `DevicePositionAssignment`: `DeviceId` → `PositionId` with history (`AssignedAtUtc`, `UnassignedAtUtc`, `IsCurrent`).\n+- `PositionUserAssignment`: `PositionId` → `TajamarUserId` with history (same fields).\n+\n+### Check-ins
- `CheckInRecord`: `TajamarUserId`, `CourseId` (optional depending on your flow), `PositionId`, `DeviceId`, `CheckedInAtUtc`, `ObservedIp`.

## API + MVC responsibilities (clean separation)
- **API** is the source of truth for:\n+  - devices, positions, assignments, check-in records\n+- **MVC**:\n+  - sets/reads the `DeviceId` cookie\n+  - calls API endpoints\n+  - provides student and admin UI

## Integration points / files likely to change
- API:
  - Add controllers:\n+    - `[ApiProyectoExcel/Controllers/CheckInsController.cs](ApiProyectoExcel/Controllers/CheckInsController.cs)`\n+    - `[ApiProyectoExcel/Controllers/DevicesController.cs](ApiProyectoExcel/Controllers/DevicesController.cs)` (admin list/assign/history)\n+  - Add DTOs:\n+    - `[Attendance.Infrastructure/DTOs/CheckInDtos.cs](Attendance.Infrastructure/DTOs/CheckInDtos.cs)`\n+    - `[Attendance.Infrastructure/DTOs/DeviceDtos.cs](Attendance.Infrastructure/DTOs/DeviceDtos.cs)`\n+    - `[Attendance.Infrastructure/DTOs/PositionDtos.cs](Attendance.Infrastructure/DTOs/PositionDtos.cs)`\n+  - Add entities + DbSets + migrations:\n+    - `[Attendance.Infrastructure/Entities/Device.cs](Attendance.Infrastructure/Entities/Device.cs)`\n+    - `[Attendance.Infrastructure/Entities/Position.cs](Attendance.Infrastructure/Entities/Position.cs)`\n+    - `[Attendance.Infrastructure/Entities/DevicePositionAssignment.cs](Attendance.Infrastructure/Entities/DevicePositionAssignment.cs)`\n+    - `[Attendance.Infrastructure/Entities/PositionUserAssignment.cs](Attendance.Infrastructure/Entities/PositionUserAssignment.cs)`\n+    - `[Attendance.Infrastructure/Entities/CheckInRecord.cs](Attendance.Infrastructure/Entities/CheckInRecord.cs)`\n+    - `[Attendance.Infrastructure/Data/ApplicationDbContext.cs](Attendance.Infrastructure/Data/ApplicationDbContext.cs)`
- MVC:
  - Student check-in:\n+    - `[ProyectoExcel/Controllers/CheckInController.cs](ProyectoExcel/Controllers/CheckInController.cs)` + `[ProyectoExcel/Views/CheckIn/Index.cshtml](ProyectoExcel/Views/CheckIn/Index.cshtml)`\n+  - Admin UI:\n+    - `[ProyectoExcel/Controllers/Admin/DevicesController.cs](ProyectoExcel/Controllers/Admin/DevicesController.cs)` + views under `[ProyectoExcel/Views/Admin/Devices/](ProyectoExcel/Views/Admin/Devices/)`\n+  - API client additions:\n+    - extend `[ProyectoExcel/Services/AttendanceApiClient.cs](ProyectoExcel/Services/AttendanceApiClient.cs)` or introduce a focused client (e.g. `DeviceApiClient`) using existing JWT forwarding (`ApiTokenHandler`).\n+  - Device cookie issuance:\n+    - add a small MVC middleware/filter to ensure `DeviceId` cookie exists (or set it after calling the API “register device” endpoint).

## Validation rules
- **DeviceIdentifier**: GUID string.\n+- **Position codes**: `ClassCode` like `T\d{2}`, `DeviceCode` like `W\d{2}`.\n+- **Authorization**:\n+  - Admin endpoints require admin/teacher role.\n+  - Student check-in requires:\n+    - student is enrolled in the course (if `courseId` is part of check-in)\n+    - the resolved Position is assigned to that student (Position→User assignment).

## Test plan
- Manual:
  - Login as student in MVC.
  - Visit check-in page: verify a `DeviceId` cookie exists and the API has a `Device` row (FirstSeen/LastSeen updated).\n+  - As admin: open devices list, pick the device, assign it to position `T38/W19`.\n+  - As admin: assign position `T38/W19` to the student.\n+  - As student: click check-in, verify API stores a `CheckInRecord` with correct `tajamar_user_id`, `deviceId`, `positionId`, and `observedIp`.\n+  - Negative case: login as another student on same device, ensure check-in is rejected (position assigned to someone else).

## Default choices (can be adjusted later)
- **Primary ID**: cookie `DeviceIdentifier` GUID.\n+- **Fallback**: IP-based heuristic only when cookie missing (and always store IP for audit).\n+- **Idempotency**: one check-in per course per day (upsert) to avoid spam.\n+- **Automatic hostname**: not required for this design; can be added later as an extra device attribute if you deploy a helper.
