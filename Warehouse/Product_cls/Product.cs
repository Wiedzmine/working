using System;
using System.Collections.Generic;

namespace Domain
{
    /*
     * this class contains the description of a product entity and warehouse. 
     * 
    */
   public enum TypeOfGoods { eatable, noteatable };

   public class Product
    {
    
        public int ID;
        public string name;
        public TypeOfGoods type;

        public float volume;
        public Product()
        {
            ID = 1;
        }
        public Product(string name,TypeOfGoods _t, float _vol)
        {
            ID++;
            this.name = name;
            this.type = _t;
            if (_vol>0) this.volume = _vol;
        }
    }

   public class Warehouse
   {
       public  int ID;

       public string name;
       public List<Product> goods;
       public float capacity;

       public Warehouse()
       {
           goods = new List<Product>();
       }

       public Warehouse(string name, float capacity)
       {
           ID++;
           this.name = name;
           this.capacity = capacity;
           this.goods = new List<Product>();
       }

       public float Busy()
       {
           float total = 0;
           foreach (Product p in goods)
           {
               total += p.volume;
           }
            return total;
       }

       public void Update(Product one) { }
       public bool isAPlace(Product some)
       {
           float free = this.capacity - this.Busy();
           try
           {
               if (free<some.volume) return true; else return false;
           }
           catch (Exception) { return false; }
       }

       public bool Push(Product one)
       {
           try
           {
               if (!this.isAPlace(one))
               {
                   this.goods.Add(one);
                   return true;
               }
               else return false;
           }
           catch (Exception) { return false; }
       }

       public void Pop(Product one)
       {
           goods.Remove(one);
       }
   }
    
}
