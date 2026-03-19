using System.Threading;
using System.Threading.Tasks;

namespace InventoryManagement.Application.Common.Interfaces;

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}