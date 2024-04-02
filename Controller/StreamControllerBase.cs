using Microsoft.AspNetCore.Mvc;

namespace CamControl.Controller
{
    public abstract class StreamControllerBase : ControllerBase
    {
        private static List<Recipient> ActiveRecipients = new List<Recipient>();
        private static double InactivityTimeoutMinutes = 5;

        /// <summary>
        /// Heartbeats the specified client identifier.
        /// </summary>
        /// <param name="clientId">The client identifier.</param>
        /// <returns></returns>
       [HttpPost("Heartbeat")]
        public IActionResult Heartbeat(Guid clientId)
        {
            var recipient = ActiveRecipients.FirstOrDefault(r => r.ClientId == clientId);
            if (recipient != null)
            {
                recipient.LastHeartbeat = DateTime.UtcNow;
            }
            else
            {
                ActiveRecipients.Add(new Recipient() { ClientId = clientId, LastHeartbeat = DateTime.Now });
            }
            // Return a response (e.g., OK)
            return Ok();
        }

        /// <summary>
        /// Adds the recipient.
        /// </summary>
        [HttpGet("GetClientId")]
        public Guid GetClientId()
        {
            Guid clientId = Guid.NewGuid();
            ActiveRecipients.Add(new Recipient() { ClientId = clientId, LastHeartbeat = DateTime.Now });
            return clientId;
        }
        /// <summary>
        /// Checks the inactive recipients.
        /// </summary>
        public async Task CheckInactiveRecipients()
        {
            while (true)
            {
                await Task.Delay(TimeSpan.FromMinutes(1)); // Adjust the interval as needed

                // Check for inactive recipients
                var now = DateTime.UtcNow;
                var inactiveRecipients = ActiveRecipients
                    .Where(r => (now - r.LastHeartbeat).TotalMinutes > InactivityTimeoutMinutes)
                    .ToList();

                foreach (var recipient in inactiveRecipients)
                {
                    ActiveRecipients.Remove(recipient);
                    // Handle cleanup or other actions if needed
                }
                if (ActiveRecipients.Count == 0)
                {
                    await NoRecipientsEvent();
                }
            }
        }

        /// <summary>
        /// Noes the recipients event.
        /// </summary>
        public virtual async Task NoRecipientsEvent() {  }
    }

    public class Recipient
    {
        public Guid ClientId { get; set; }
        public DateTime LastHeartbeat { get; set; }
    }
}
