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
                while(occupied == size)
                  Monitor.Wait(this);
                buffer[index] = obj;
                index = ++index % size;
                occupied++;
            }
        }

        public T consume(){
            T obj;
            lock(this){
                while(occupied == 0)
                    Monitor.Wait(this);
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