using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace D3DLab.Core.Input {
    public enum ImputContextTypes {
        /// <summary>
        /// An action is a single-time thing, like casting a spell or opening a door; generally if the player just holds the button down, the action should only happen once, generally when the button is first pressed, or when it is finally released. "Key repeat" should not affect actions.
        /// </summary>
        Actions,
        /// <summary>
        /// States are similar, but designed for continuous activities, like running or shooting. A state is a simple binary flag: either the state is on, or it's off. When the state is active, the corresponding game action is performed; when it is not active, the action is not performed. Simple as that. Other good examples of states include things like scrolling through menus.
        /// </summary>
        States,
        /// <summary>
        /// A range is an input that can have a number value associated with it. For simplicity, we will assume that ranges can have any value; however, it is common to define them in normalized spans, e.g. 0 to 1, or -1 to 1. We'll see more about the specifics of range values later. Ranges are most useful for dealing with analog input, such as joysticks, analog controller thumbsticks, and mice.
        /// </summary>
        Ranges
    }
    class ImputContext {
    }

    class RawImputMapper {
        
    }


}
