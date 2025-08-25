namespace BillIssue.Web.Services
{
    public class NavScrollService
    {
        public event Action<bool>? ScrollModeChanged;

        private bool isVertical = true;

        public bool IsVertical
        {
            get => isVertical;
            set
            {
                if (isVertical != value)
                {
                    isVertical = value;
                    ScrollModeChanged?.Invoke(isVertical);
                }
            }
        }
    }
}