using System.Collections.Generic;
using System.Collections.Immutable;

namespace rache_der_reti.Core.Messages
{
    public class HudMessages
    {
        public readonly ImmutableDictionary<string, string> mMessageDictionary;

        public HudMessages()
        {
            mMessageDictionary = new Dictionary<string, string>
            {
                // Tutorial messages.
                {"tutorial_info", "Deactivate/activate tutorial messages by pressing F1."},
                {"tutorial_off", "You deactivated tutorial messages."},
                {"tutorial_on", "You activated tutorial messages."},
                // Tutorial messages objects.
                {"hacker", "The blue guy is your hacker. He can open and close doors with a LEFT-CLICK. Protect him well!"},
                {"warrior", "The red guy is your warrior. Press SPACE to use his EMP against your enemies."},
                {"scout", "This cute duck is your scout. It's quick and will not be attacked by the ReTI."},
                {"terminal", "This is an active terminal. LEFT-CLICK on it to collect code-snippets for the antivirus."},
                {"door", "Your hacker can open or close doors with a LEFT-CLICK. Try it!"},
                {"emp", ""},
                // Tutorial messages other.
                {"single_select", "Select a single hero with TAB or by clicking on it."},
                {"mouse_drag", "Drag a rectangle with the mouse to select multiple heroes."},
                {"radar", "<= The radar on the left tells you where to find the next active terminal."},
                // Game events
                {"debug_on", "Debug mode is activated"},
                {"debug_off", "Debug mode is disabled"},
                {"antivirus_built", "Antivirus Program built! Go to the ReTI and insert it!"},
                {"first_damage", "Your hero is being attacked. Run for your life or use the warrior's EMP!"},
                {"hero_died", "Oh no! Your hero died!"},
                {"hacker_died", "Oh no! Your hacker died!"},
                {"warrior_died", "Oh no! Your warrior died!"},
                {"hacker_duck", "You lost your hacker! But wait! Your duck has an IT-degree as well!"},
                {"new_zombie", "The ReTI infected another computer!"},
                {"snippets_collected", "Awesome! You collected all code-snippets from this Terminal! (GAME SAVED)"},
                {"reti_deactivated", "You deactivated the ReTI! It won't infect further computers! Go to the off-switch to finish the job!"},
                {"off_switch_used", "Well done! You destroyed the ReTI and saved the world! At least for now..."}
            }.ToImmutableDictionary();
        }
    }
}
