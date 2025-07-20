namespace Energy8.Identity.UI.Runtime.ViewModels
{
    public class UserViewModel
    {
        public string Title { get; }
        
        public UserViewModel(string title = "Welcome")
        {
            Title = title;
        }
        
        public static UserViewModel Default => new UserViewModel();
    }
}
