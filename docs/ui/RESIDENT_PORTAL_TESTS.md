# Resident Portal Verification Points

1. Authenticate through PortalCorporativo with `appcondominio.resident.access`.
2. Confirm `/resident` loads only units associated with the JWT user through active `People.AccessGrant.ExternalUserId` mappings.
3. Change a unitId in the browser route/API call to a unit not granted to the user and verify the API rejects access.
4. Confirm the unit card shows property code, location and main area rather than technical identifiers.
5. Confirm statement and receivables contain only the selected authorized unit.
6. Confirm reservable areas are listed only from the resident community.
7. Attempt to submit an area from another community and verify the API rejects the request.
8. Create a valid reservation and confirm the requester is the authenticated resident PersonId.
9. Authorize a visitor and confirm CommunityId/PersonId are server-resolved, not accepted from the browser.
10. Remove or expire the resident AccessGrant and verify the portal no longer exposes the unit.
