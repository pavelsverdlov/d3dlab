using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace D3DLab.Debugger {

    public interface IAutomationEntity {

    }
    public interface IAutomationViewport {
        IAutomationEntity Get(Guid guid);
        void Add(IAutomationEntity entity);
        void Remove(IAutomationEntity entity);
        bool Exist(IAutomationEntity entity);
    }


    public class Main {

        public void Test() {
            IAutomationViewport viewport = null;

            IAutomationEntity entity = viewport.Get(Guid.Empty);

            //


        }
    }
}
