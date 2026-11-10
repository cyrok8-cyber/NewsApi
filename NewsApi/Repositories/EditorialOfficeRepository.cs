using Microsoft.EntityFrameworkCore;
using NewsApi.Data;
using NewsApi.Entities;

namespace NewsApi.Repositories
{
    public class EditorialOfficeRepository : IEditorialOfficeRepository
    {
        private readonly AppDbContext _context;

        public EditorialOfficeRepository(AppDbContext context)
        {
            _context = context;
        }

        // Все редакции вместе с журналистами, по алфавиту
        public async Task<List<EditorialOffice>> GetAllAsync()
        {
            return await _context.EditorialOffices
                .Include(o => o.Members)
                .OrderBy(o => o.Name)
                .ToListAsync();
        }

        public async Task<EditorialOffice?> GetByIdAsync(int id)
        {
            return await _context.EditorialOffices
                .Include(o => o.Members)
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        // Нужен для проверки уникальности наименования
        public async Task<EditorialOffice?> GetByNameAsync(string name)
        {
            return await _context.EditorialOffices
                .FirstOrDefaultAsync(o => o.Name == name);
        }

        public async Task<EditorialOffice> CreateAsync(EditorialOffice office)
        {
            _context.EditorialOffices.Add(office);
            await _context.SaveChangesAsync();
            return office;
        }

        public async Task UpdateAsync(EditorialOffice office)
        {
            _context.EditorialOffices.Update(office);
            await _context.SaveChangesAsync();
        }

        // Журналисты удаляемой редакции становятся одиночками,
        // у новостей пропадает ссылка на редакцию (DeleteBehavior.SetNull в AppDbContext)
        public async Task DeleteAsync(EditorialOffice office)
        {
            _context.EditorialOffices.Remove(office);
            await _context.SaveChangesAsync();
        }
    }
}
