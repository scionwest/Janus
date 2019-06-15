using Janus.Seeding;

namespace Janus.SampleApi
{
    public class UserTaskCollection : SeederCollection
    {
        public UserTaskCollection()
        {
            this.AddSeeder<UserEntitySeeder>();
            this.AddSeeder<TaskEntitySeeder>();
        }
    }
}
