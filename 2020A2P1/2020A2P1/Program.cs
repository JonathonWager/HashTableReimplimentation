/*CIOS 2020H
 * Assignment #2 Part A
 * 11/13/2022
 * Group members:
 * Jonathon Wager
 * Vu Anh Thu Nguyen
 * Saran Chowdhury
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenHashTable
{
    public class Point : IComparable
    {
        //Point Data Members
        public int x { get; set; }
        int y{ get; set; }

        //Point Constructor 
        public Point(int x,int y)
        {
            this.x = x;
            this.y = y;
        }
        
        //CompareTo for two points
        //Returns 0 if object is null == to current point
        //Returns 1 if current point is greater than obj
        //returns -1 if current point is less than obj
        public int CompareTo(Object obj)
        { 
            Point p;
            if (obj == null)
            {   
                return 0;
            }
            else
            { 
                p = (Point)obj;
            }
            if (x == p.x)
            {
                if (y == p.y)
                {
                    return 0;
                }
                else if (y > p.y)
                {
                    return 1;
                }
                else
                {
                    return -1;
                }
            }
            else if (x > p.x)
            {   
                return 1;
            }     
            return -1;    
        }
        //Equals override for Point
        //returns true if Points are equal and false if they are not
        public override bool Equals(Object obj)
        {
            if (!(obj is Point))
            {
                return false;
            }
            Point p = (Point)obj;
            return x == p.x & y == p.y;
        }
        //Get hashcode override for points
        public override int GetHashCode()
        {
            return x ^ y;
        }
        //to string override for points
        public override string ToString()
        {
            return "(" + x + "," + y +")";
        }
    }
    public interface IHashTable<TKey, TValue>
    {
        void Insert(TKey key, TValue value);   // Insert a <key,value> pair
        bool Remove(TKey key);                 // Remove the value with key
        TValue Retrieve(TKey key);             // Return the value of a key
    }

    //---------------------------------------------------------------------------------------

    public class HashTable<TKey, TValue> : IHashTable<TKey, TValue> where TKey : IComparable
    {

        private class Node
        {
            // <key,value> pair (item)
            public TKey key;
            public TValue value;

            public Node next;

            public Node(TKey key, TValue value, Node next = null)
            {
                this.key = key;
                this.value = value;
                this.next = next;
            }
        }

        // Data Members

        private Node[] HT;                // Hash table array of nodes
        private int numBuckets;           // Number of buckets
        private int numItems;             // Number of items

        // Constructor
        // Creates an empty hash table

        public HashTable()
        {
            numBuckets = 1;
            HT = new Node[numBuckets];
            MakeEmpty();
        }

        // MakeEmpty
        // Sets all buckets to empty

        public void MakeEmpty()
        {
            int i;

            for (i = 0; i < numBuckets; i++)
                HT[i] = null;
            numItems = 0;
        }

        // NextPrime
        // Returns the next prime number > k

        private int NextPrime(int k)
        {
            int i;
            bool prime = false;

            // Begin at an odd number
            if (k == 1 || k == 2)
                return k + 1;

            if (k % 2 == 0) k++;

            while (!prime)
            {
                // Divide k by odd factors
                for (i = 3; i * i < k && k % i != 0; i += 2) ;

                if (k % i != 0)
                    prime = true;
                else
                    // Increase k to the next odd number
                    k += 2;
            }
            return k;
        }

        // Rehash
        // Doubles the size of the hash table to the next highest prime number
        // Rehashes the items from the original hash table

        private void Rehash()
        {
            int i, k;
            int temp = numBuckets;       // Store the old number of buckets and
            Node[] tempHT = HT;          // hash table array

            // Determine the capacity of the new hash table
            numBuckets = NextPrime(2 * numBuckets);

            // Create the new hash table array and initialize each bucket to empty
            HT = new Node[numBuckets];
            MakeEmpty();

            // Rehash items from the old to new hash table
            for (i = 0; i < temp; i++)
            {
                if (tempHT[i] != null)
                {
                    Node p = tempHT[i].next;
                    while (p != null)
                    {
                        Insert(p.key, p.value);
                        p = p.next;
                    }
                }
            }
        }

        // Insert
        // Insert a <key,value> into the current hash table
        // If the key is already found, an exception is thrown

        public void Insert(TKey key, TValue value)
        {
            //get hash code from key
            int i = key.GetHashCode() % numBuckets;     
            Node curr;
            //check if bucket is empty
            if (HT[i] !=null)
            {
                //if bucket isnt empty check for duplicate keys
                curr = HT[i].next;
                while (curr != null)
                {
                    // Unsuccessful insert (key found already)
                    if (curr.key.Equals(key))
                        throw new InvalidOperationException("Duplicate key");
                    else
                        curr = curr.next;
                }
            }
            else
            {
                //if bucket is empty add header node to it
                HT[i] = new Node(default, default, null);
            }           
            //curr node gets set to header node .next
            curr = HT[i].next;

            //inserted set to false 
            bool inserted = false;

            //prev node gets set to header node
            Node prev = HT[i];
            while (!inserted)
            {
                if (curr == null)
                {
                    //if there is only a header node set the header nodes next to new node
                    prev.next = new Node(key, value, null);
                    inserted = true;
                }
                else
                {
                    if (key.CompareTo(curr.key) < 0)
                    {
                        //if key is less than current nodes key
                        //add key and value in to the list at this point
                        prev.next = new Node(key, value, curr);
                        inserted = true;
                    }
                    if (key.CompareTo(curr.key) > 0)
                    {
                        //if key is greator than current nodes key move to next node
                        prev = curr; curr = curr.next;
                    }
                }
            }

            numItems++;
            // Rehash if the average size of the buckets exceeds 5.0
            if ((double)numItems / numBuckets > 5.0)
                Rehash();
        }

        // Remove
        // Delete (if found) the <key,value> with the given key
        // Return true if successful, false otherwise

        public bool Remove(TKey key)
        {
            int i = key.GetHashCode() % numBuckets;
            if(HT[i] != null)
            {
                Node p = HT[i].next;
                if (p == null)
                    return false;
                else
                // Successful remove of the first item in a bucket
                if (p.key.Equals(key))
                {
                    HT[i] = HT[i].next;
                    numItems--;
                    return true;
                }
                else
                    while (p.next != null)
                    {
                        // Successful remove (<key,value> found and deleted)
                        if (p.next.key.Equals(key))
                        {
                            p.next = p.next.next;
                            numItems--;
                            return true;
                        }
                        else
                            p = p.next;
                    }
            }

            // Unsuccessful remove (key not found)
            return false;
        }

        // Retrieve
        // Returns (if found) the value of the given key
        // If the key is not found, an exception is thrown

        public TValue Retrieve(TKey key)
        {
            int i = key.GetHashCode() % numBuckets;
            if(HT[i] != null)
            {
                Node p = HT[i].next;
                while (p != null)
                {
                    // Successful retrieval (value found and returned)
                    if (p.key.Equals(key))
                        return p.value;
                    else
                        p = p.next;
                }
            }           
            throw new InvalidOperationException("Key not found");
        }

        // Print
        // Prints the hash table entries, one line per bucket

        public void Print()
        {
            int i;
            Node p;
            for (i = 0; i < numBuckets; i++)
            {
                Console.Write(i.ToString().PadLeft(2) + ": ");
                if (HT[i] != null)
                {
                    p = HT[i].next;
                    while (p != null)
                    {
                        Console.Write("<" + p.key.ToString() + "," + p.value.ToString() + "> ");
                        p = p.next;
                    }
                    Console.WriteLine();
                }
                Console.WriteLine();

            }
        }

        public void Output()
        {
            //array to store all nodes in buckets
            Node[] nodes = new Node[numItems];
            //count to count how many items we get
            int count = 0;
            //compare count for sorting loop
            int compareCount = 0;
            //index node and storage node
            Node p;
            Node store;

            //getting all nodes in buckets that are not null and storing them in nodes array
            for (int i = 0; i < numBuckets; i++)
            {
                if (HT[i] != null)
                {
                    p = HT[i].next;
                    while (p != null)
                    {
                        nodes[count] = p;
                        count++;
                        p = p.next;
                    }
                }
            }

            //subtract 1 from count for index
            count = count - 1;

            //sorts array of nodes lowest to highest
            while (count > 0)
            {
                if (compareCount == nodes.Length - 1 || compareCount >= count)
                {
                    count--;
                    compareCount = 0;
                }
                if (nodes[count].key.CompareTo(nodes[compareCount].key) < 0)
                {
                    store = nodes[count];
                    nodes[count] = nodes[compareCount];
                    nodes[compareCount] = store;
                }
                compareCount++;
            }
            //prints out keys and values in order
            for(int i = 0; i < nodes.Length; i++)
            {
                Console.WriteLine("Key= " + nodes[i].key + " Value= " + nodes[i].value);
            }
        }
    }
    //---------------------------------------------------------------------------------------

    class Program
    {
        static void Main(string[] args)
        {
            HashTable<Point, int> H = new HashTable<Point, int>();

            Console.WriteLine("Executing Open Hash Table");
            Console.WriteLine();
            Random rnd = new Random();
            for (int i = 0; i < 20; i++)
            {
                H.Insert(new Point(rnd.Next(1, 90), rnd.Next(1, 90)), rnd.Next(1, 100));
            }
            H.Print();
            H.Insert(new Point(0, 222), 22);
            Console.WriteLine(H.Remove(new Point(0, 222)));
            for (int i = 1; i < 20; i++)
                Console.WriteLine(H.Remove(new Point(rnd.Next(1, 90), rnd.Next(1, 90))));

            //H.Print();
            H.Output();
            Console.ReadKey();
        }
    }
}
