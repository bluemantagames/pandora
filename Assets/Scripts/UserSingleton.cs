namespace Pandora
{
    public class UserSingleton
    {
        public string Token { get; set; } = null;

        private static UserSingleton privateInstance = null;

        public static UserSingleton instance
        {
            get
            {
                if (privateInstance == null)
                {
                    privateInstance = new UserSingleton();
                }

                return privateInstance;
            }
        }

        private UserSingleton() { }
    }
}