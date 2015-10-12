using System;
using System.Threading;

namespace App {
    class Buffer<T> {

        private T[] buffer;
        private int index;
        private int size;
        private int occupied;
        private int exec;

        public Buffer(int sizeofBuffer){
            buffer = new T[sizeofBuffer];
            size = sizeofBuffer;
            index = occupied = exec = 0;
        }

        public void produce(T obj){
            lock(this){
                while (occupied == size)
                {
                    Console.WriteLine("Thread will wait at produce");
                    Monitor.Wait(this);
                    Console.WriteLine("Thread awake at produce");
                }
                buffer[index] = obj;
                index = ++index % size;
                occupied++;
                Monitor.Pulse(this);
            }

        }

        public T consume(){
            T obj;
            lock(this){
                while (occupied == 0)
                {
                    Console.WriteLine("Thread will wait at consume");
                    Monitor.Wait(this);
                    Console.WriteLine("Thread awake at consume");

                }
                obj = buffer[exec];
                buffer[exec] = default(T);
                exec = ++exec % size;
                occupied--;
                Monitor.Pulse(this);
            }
            return obj;
        }

        public void showStatus(){

            for(int i = 0; i < size; i++)
                Console.WriteLine("Buffer[{0}] = {1}", i, buffer[i]);

            Console.WriteLine("size = {0} : index = {1} : occupied = {2} : exec = {3}", size, index, occupied, exec);
        }

    }


}