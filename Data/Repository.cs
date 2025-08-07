using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace WpfApp1.Data
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly string _filePath;
        private readonly object _lock = new object();

        public Repository(string filePath)
        {
            _filePath = filePath;
            Directory.CreateDirectory(Path.GetDirectoryName(_filePath));
            if (!File.Exists(_filePath))
                File.WriteAllText(_filePath, "[]");
        }

        private List<T> Load()
        {
            lock (_lock)
            {
                var json = File.ReadAllText(_filePath);
                return JsonConvert.DeserializeObject<List<T>>(json) ?? new List<T>();
            }
        }

        private void Save(List<T> data)
        {
            lock (_lock)
            {
                var json = JsonConvert.SerializeObject(data, Formatting.Indented);
                File.WriteAllText(_filePath, json);
            }
        }

        public Task<List<T>> GetAllAsync() => Task.FromResult(Load());

        public Task<T> GetByIdAsync(int id)
        {
            var prop = typeof(T).GetProperty("Id");
            var result = Load().FirstOrDefault(x => (int)prop.GetValue(x) == id);
            return Task.FromResult(result);
        }

        public Task AddAsync(T entity)
        {
            var list = Load();
            var prop = typeof(T).GetProperty("Id");
            var nextId = list.Any() ? list.Max(x => (int)prop.GetValue(x)) + 1 : 1;
            prop.SetValue(entity, nextId);
            list.Add(entity);
            Save(list);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(T entity)
        {
            var prop = typeof(T).GetProperty("Id");
            var id = (int)prop.GetValue(entity);
            var list = Load();
            var idx = list.FindIndex(x => (int)prop.GetValue(x) == id);
            if (idx >= 0)
            {
                list[idx] = entity;
                Save(list);
            }
            return Task.CompletedTask;
        }

        public Task DeleteAsync(int id)
        {
            var prop = typeof(T).GetProperty("Id");
            var list = Load().Where(x => (int)prop.GetValue(x) != id).ToList();
            Save(list);
            return Task.CompletedTask;
        }
    }
}