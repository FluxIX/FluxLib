/* *
 * Copyright (C) 2015 Christopher Herrick
 * 
 * This file is part of the FluxLib library.
 *
 * The FluxLib library is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Lesser General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.

 * The FluxLib library is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Lesser General Public License for more details.

 * You should have received a copy of the GNU Lesser General Public License
 * along with the FluxLib library.  If not, see <http://www.gnu.org/licenses/>.
 * */

using System;
using System.Collections;
using System.Collections.Generic;

namespace FluxLib.Collections.Generic
{
   public class ReverseEnumerator<T> : IEnumerator<T>, IDisposable
   {
      public ReverseEnumerator( IEnumerator<T> source, Boolean resetSource = true )
      {
         Initialize( source, resetSource );
      }

      protected void Initialize( IEnumerator<T> source, Boolean resetSource )
      {
         Items = new List<T>();

         if( resetSource )
            source.Reset();

         while( source.MoveNext() )
            Items.Add( source.Current );

         Reset();
      }

      protected IList<T> Items
      {
         get;
         set;
      }

      protected Int32 CurrentIndex
      {
         get;
         set;
      }

      protected T CurrentItem
      {
         get;
         set;
      }

      public Boolean Completed
      {
         get
         {
            return CurrentIndex <= 0;
         }
      }

      public virtual void Reset()
      {
         CurrentIndex = Items.Count;
         CurrentItem = default( T );
      }

      public virtual void Dispose()
      {
         Items.Clear();
         Reset();
      }

      public virtual T Current
      {
         get
         {
            return CurrentItem;
         }
      }

      Object IEnumerator.Current
      {
         get
         {
            return CurrentItem;
         }
      }

      public virtual Boolean MoveNext()
      {
         Boolean result;

         if( result = !Completed )
            CurrentItem = Items[ --CurrentIndex ];
         else
            CurrentItem = default( T );

         return result;
      }
   }
}
