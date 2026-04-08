using SQLite;
using Downkyi.Core.Storage;

namespace Downkyi.Core.Database.Download;

public class DownloadDatabase
{
    private readonly string _dbPath = StorageManager.GetDownload();
    private SQLiteAsyncConnection? _db;
    private static DownloadDatabase? _instance;
    private static readonly object _lock = new();

    private DownloadDatabase() { }

    public static DownloadDatabase Instance
    {
        get
        {
            if (_instance == null) lock (_lock) { _instance ??= new DownloadDatabase(); }
            return _instance;
        }
    }

    private async Task InitAsync()
    {
        if (_db != null) return;
        var options = new SQLiteConnectionString(_dbPath, true, key: "d0wnky1-dl-2024");
        _db = new SQLiteAsyncConnection(options);
        await _db.CreateTableAsync<DownloadingEntity>();
        await _db.CreateTableAsync<DownloadedEntity>();
    }

    // --- Downloading ---

    public async Task<int> InsertDownloadingAsync(DownloadingEntity entity)
    {
        await InitAsync();
        entity.CreatedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        return await _db!.InsertAsync(entity);
    }

    public async Task<int> UpdateDownloadingAsync(DownloadingEntity entity)
    {
        await InitAsync();
        return await _db!.UpdateAsync(entity);
    }

    public async Task<int> DeleteDownloadingAsync(string uuid)
    {
        await InitAsync();
        return await _db!.Table<DownloadingEntity>().DeleteAsync(x => x.Uuid == uuid);
    }

    public async Task<List<DownloadingEntity>> GetAllDownloadingAsync()
    {
        await InitAsync();
        return await _db!.Table<DownloadingEntity>().ToListAsync();
    }

    public async Task<DownloadingEntity?> GetDownloadingByUuidAsync(string uuid)
    {
        await InitAsync();
        return await _db!.Table<DownloadingEntity>().Where(x => x.Uuid == uuid).FirstOrDefaultAsync();
    }

    // --- Downloaded ---

    public async Task<int> InsertDownloadedAsync(DownloadedEntity entity)
    {
        await InitAsync();
        entity.FinishedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        return await _db!.InsertAsync(entity);
    }

    public async Task<int> DeleteDownloadedAsync(long id)
    {
        await InitAsync();
        return await _db!.Table<DownloadedEntity>().DeleteAsync(x => x.Id == id);
    }

    public async Task<List<DownloadedEntity>> GetAllDownloadedAsync()
    {
        await InitAsync();
        return await _db!.Table<DownloadedEntity>().OrderByDescending(x => x.FinishedAt).ToListAsync();
    }

    public async Task<bool> IsAlreadyDownloadedAsync(string bvid)
    {
        await InitAsync();
        return await _db!.Table<DownloadedEntity>().Where(x => x.Bvid == bvid).CountAsync() > 0;
    }
}
