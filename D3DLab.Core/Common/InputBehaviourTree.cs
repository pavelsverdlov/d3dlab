using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace D3DLab.Core.Components {
    public enum Types {
        SUCCESS,// returned when a criterion has been met by a condition node or an action node has been completed successfully;
        FAILURE,// returned when a criterion has not been met by a condition node or an action node could not finish its execution for any reason;
        RUNNING,// returned when an action node has been initialized but is still waiting the its resolution.
        ERROR //returned when some unexpected error happened in the tree, probably by a programming error (trying to verify an undefined variable). Its use depends on the final implementation of the leaf nodes.
    }
    public class InputState {
        
    }
    public class InputBehaviourTree {
    }

    /// <summary>
    /// The leaf nodes are the primitive building blocks of the behavior tree. 
    /// These nodes do not have any child, thus, they do not propagate the tick signal,
    /// instead, they perform some computation and return a state value. 
    /// There are two types of leaf nodes (conditions and actions) and are categorized by their responsibility.
    /// </summary>
    public class InputLeafNode {
        
    }

    public class InputCondition : InputLeafNode {
        public bool Handle() { return true;}
    }

    public class InputAction : InputLeafNode {
        public bool Handle() { return true; }
    }

    public class InputCompositeNode { }

}
