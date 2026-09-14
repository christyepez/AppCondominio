using AppCondominio.Modules.Reservations.Domain;using AppCondominio.Modules.SecurityOperations.Domain;
namespace AppCondominio.UnitTests;
public sealed class ReservationsSecurityDomainTests
{
 [Fact] public void Reservation_rejects_invalid_range(){Assert.Throws<ArgumentException>(()=>Reservation.Create(Guid.NewGuid(),Guid.NewGuid(),Guid.NewGuid(),DateTimeOffset.UtcNow,DateTimeOffset.UtcNow.AddMinutes(-1),1,0));}
 [Fact] public void Visitor_checkin_requires_valid_window(){var from=DateTimeOffset.UtcNow.AddHours(1);var x=VisitorAuthorization.Create(Guid.NewGuid(),Guid.NewGuid(),"Visitor","ID1","A-101",from,from.AddHours(2));Assert.Throws<InvalidOperationException>(()=>x.CheckIn(DateTimeOffset.UtcNow,"Gate 1"));}
 [Fact] public void Security_incident_can_escalate_and_resolve(){var x=SecurityIncident.Create(Guid.NewGuid(),"Access","Unauthorized attempt","Gate","guard");x.Escalate();x.Resolve("Verified and closed");Assert.Equal(SecurityIncidentStatus.Resolved,x.Status);}
}