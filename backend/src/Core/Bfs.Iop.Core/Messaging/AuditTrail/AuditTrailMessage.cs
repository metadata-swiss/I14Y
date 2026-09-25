using Bfs.Iop.AuditTrail.Abstractions.Models;
using System.ComponentModel.DataAnnotations;

namespace Bfs.Iop.Core.Messaging.AuditTrail;

internal sealed record AuditTrailMessage([Required]CommitRequest CommitRequest, int FailCount = 0)
{ }
