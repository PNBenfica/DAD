using System;
using System.Threading;

delegate void ThrWork();
namespace App{
    class ThrPool
    {
        private Buffer<ThrWork> buffer;
        private Thread[] poolThread;
        public ThrPool(int thrNum, int bufSize)
        {
               poolThread = new Thread[thrNum];
               buffer = new Buffer<ThrWork>(bufSize);	
               for(int index = 0; index < thrNum; index++){
                    poolThread[index] = new Thread(new ThreadStart(consumeDelegates));
                    poolThread[index].Start();
                }   


        }

        public void AssyncInvoke(ThrWork action)
        {
            buffer.produce(action);
            // TODO
        }

        public void consumeDelegates(){
            while(true){
                ThrWork obj = buffer.consume();
                obj();
            }
                
        }
    }


    class A
    {
        private int _id;

        public A(int id)
        {
            _id = id;
        }

        public void DoWorkA()
        {
            Console.WriteLine("A-{0}", _id);
        }
    }


    class B
    {
        private int _id;

        public B(int id)
        {
            _id = id;
        }

        public void DoWorkB()
        {
            Console.WriteLine("B-{0}", _id);
        }
    }

    
    class Test
    {
        public static void Main()
        {
            ThrPool tpool = new ThrPool(5, 10);
            ThrWork work = null;
            for (int i = 0; i < 5; i++)
            {
                A a = new A(i);
                tpool.AssyncInvoke(new ThrWork(a.DoWorkA));
                B b = new B(i);
                tpool.AssyncInvoke(new ThrWork(b.DoWorkB));
            }
            Console.ReadLine();
        }
    }
}