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
using System.Text;

namespace FluxLib.Collections.Generic.Tree
{
   public interface IGeneralTree<NodeValueType, ConcreteTreeType, ConcreteTreeNodeType> : ITree<NodeValueType, ConcreteTreeType, ConcreteTreeNodeType>
      where ConcreteTreeType : IGeneralTree<NodeValueType, ConcreteTreeType, ConcreteTreeNodeType>
      where ConcreteTreeNodeType : IGeneralTreeNode<NodeValueType, ConcreteTreeNodeType>
   {
      void InsertBefore( ConcreteTreeNodeType reference, NodeValueType value );
      void InsertBefore( ConcreteTreeNodeType reference, ConcreteTreeNodeType node );

      void InsertAfter( ConcreteTreeNodeType reference, NodeValueType value );
      void InsertAfter( ConcreteTreeNodeType reference, ConcreteTreeNodeType node );
   }
}
