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
using System.Collections.Generic;

namespace FluxLib.Collections.Generic.Tree
{
   public interface ITree<NodeValueType, ConcreteTreeType, ConcreteTreeNodeType> : IEquatable<ConcreteTreeType>, IEnumerable<ConcreteTreeNodeType>
      where ConcreteTreeType : ITree<NodeValueType, ConcreteTreeType, ConcreteTreeNodeType>
      where ConcreteTreeNodeType : ITreeNode<NodeValueType, ConcreteTreeNodeType>
   {
      ConcreteTreeNodeType Root
      {
         get;
      }

      Int32 Count
      {
         get;
      }

      Boolean IsEmpty
      {
         get;
      }

      void Clear();

      void InsertRoot( NodeValueType value );
      void InsertRoot( ConcreteTreeNodeType node );

      void Remove( NodeValueType value, Boolean orphanChildren );
      void Remove( ConcreteTreeNodeType node, Boolean orphanChildren );

      Boolean Contains( NodeValueType value );
      Boolean Contains( NodeValueType value, IEnumerator<ConcreteTreeNodeType> enumerator );

      ConcreteTreeNodeType Search( NodeValueType value );
      ConcreteTreeNodeType Search( NodeValueType value, IEnumerator<ConcreteTreeNodeType> enumerator );

      #region Enumeration
      IEnumerator<NodeValueType> DepthFirstNodeValueEnumerator( EnumerationDirection direction );
      IEnumerator<NodeValueType> ForwardDepthFirstNodeValueEnumerator();
      IEnumerator<NodeValueType> ReverseDepthFirstNodeValueEnumerator();
      IEnumerator<NodeValueType> BreadthFirstNodeValueEnumerator( EnumerationDirection direction );
      IEnumerator<NodeValueType> ForwardBreadthFirstNodeValueEnumerator();
      IEnumerator<NodeValueType> ReverseBreadthFirstNodeValueEnumerator();
      IEnumerator<ConcreteTreeNodeType> DepthFirstNodeEnumerator( EnumerationDirection direction );
      IEnumerator<ConcreteTreeNodeType> ForwardDepthFirstNodeEnumerator();
      IEnumerator<ConcreteTreeNodeType> ReverseDepthFirstNodeEnumerator();
      IEnumerator<ConcreteTreeNodeType> BreadthFirstNodeEnumerator( EnumerationDirection direction );
      IEnumerator<ConcreteTreeNodeType> ForwardBreadthFirstNodeEnumerator();
      IEnumerator<ConcreteTreeNodeType> ReverseBreadthFirstNodeEnumerator();
      #endregion

      IEnumerator<ConcreteTreeNodeType> DefaultNodeEnumerator
      {
         get;
         set;
      }

      IEnumerator<NodeValueType> DefaultValueEnumerator
      {
         get;
         set;
      }
   }
}
