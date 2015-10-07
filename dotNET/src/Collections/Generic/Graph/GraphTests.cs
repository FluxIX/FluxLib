/* *
 * Copyright (C) 2011, 2012 Christopher Herrick
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
using NUnit.Framework;

using FluxLib.Collections.Generic.Graph;

namespace FluxLib.Collections.Generic.Graph.Tests
{
   [TestFixture]
   public class GraphTests
   {
      protected const Double WEIGHT_TOLERANCE = .001;

      protected Graph<String> Graph
      {
         get;
         set;
      }

      [SetUp]
      public void Initialize()
      {
         Graph = new Graph<String>();
      }

      [TearDown]
      public void TearDown()
      {
         Graph.Dispose();
      }

      [Test]
      public void TestGraphNodeInstantiation()
      {
         GraphNode<String> node = new GraphNode<String>( "A" );

         Assert.AreEqual( node.Id, "A" );
         Assert.AreEqual( node.Precedessors.Count, 0 );
         Assert.AreEqual( node.Successors.Count, 0 );
      }

      [Test]
      public void TestGraphNodeEquals()
      {
         GraphNode<String> node1 = new GraphNode<String>( "A" );
         GraphNode<String> node2 = new GraphNode<String>( "B" );
         GraphNode<String> node3 = new GraphNode<String>( "A" );

         Assert.AreEqual( node1, node1 );
         Assert.AreEqual( node1, node3 );
         Assert.AreNotEqual( node1, node2 );
      }

      [Test]
      public void TestGraphNodeDispose()
      {
         GraphNode<String> node = new GraphNode<String>( "A" );
         node.Precedessors.Add( node );
         node.Successors.Add( node );

         Assert.AreEqual( node.Precedessors.Count, 1 );
         Assert.AreEqual( node.Successors.Count, 1 );

         node.Dispose();

         Assert.AreEqual( node.Precedessors.Count, 0 );
         Assert.AreEqual( node.Successors.Count, 0 );
      }

      [Test]
      public void TestGraphNodeHashCode()
      {
         GraphNode<String> node1 = new GraphNode<String>( "A" );
         GraphNode<String> node2 = new GraphNode<String>( "B" );

         Assert.AreEqual( node1.GetHashCode(), node1.GetHashCode() );
         Assert.AreNotEqual( node1.GetHashCode(), node2.GetHashCode() );
      }

      [Test]
      public void Instantiate()
      {
         Assert.AreEqual( Graph.Nodes.Count, 0 );
         Assert.AreEqual( Graph.Weights.Count, 0 );
      }

      [Test]
      public void TestDispose()
      {
         Graph.Dispose();
         Assert.AreEqual( Graph.Nodes.Count, 0 );
         Assert.AreEqual( Graph.Weights.Count, 0 );
      }

      [Test]
      public void SuccessfullyAddNode()
      {
         Graph.AddNode( "A" );

         Assert.AreEqual( Graph.Nodes.Count, 1 );
         Assert.IsTrue( Graph.Nodes.Contains( new GraphNode<String>( "A" ) ) );
      }

      [Test]
      [ExpectedException( typeof( ArgumentException ) )]
      public void UnsuccessfullyAddNode()
      {
         Graph.AddNode( "A" ).AddNode( "A" );
      }

      [Test]
      public void SuccessfullyRetrieveNode()
      {
         Graph.AddNode( "A" );
         Assert.IsNotNull( Graph.GetNode( "A" ) );
      }

      [Test]
      public void UnsuccessfullyRetrieveNode()
      {
         Assert.IsNull( Graph.GetNode( "A" ) );
      }

      [Test]
      public void DoesContainNode()
      {
         Graph.AddNode( "A" );
         Assert.IsTrue( Graph.ContainsNode( "A" ) );
      }

      [Test]
      public void DoesNotContainNode()
      {
         Assert.IsFalse( Graph.ContainsNode( "A" ) );
      }

      [Test]
      public void SuccessfullyRemoveNode()
      {
         Graph.AddNode( "A" ).RemoveNode( "A" );
         Assert.IsFalse( Graph.ContainsNode( "A" ) );
      }

      [Test]
      [ExpectedException( typeof( ArgumentException ) )]
      public void UnsuccessfullyRemoveNode()
      {
         Graph.RemoveNode( "A" ).RemoveNode( "A" );
      }

      [Test]
      public void SuccessfullyAddConnection()
      {
         Graph.AddNode( "A" ).AddNode( "B" ).AddConnection( "A", "B", 2 );
         Assert.IsTrue( Graph.Weights.ContainsKey( Graph.GetNode( "A" ) ) && Graph.Weights[ Graph.GetNode( "A" ) ].ContainsKey( Graph.GetNode( "B" ) ) && Graph.Weights[ Graph.GetNode( "A" ) ][ Graph.GetNode( "B" ) ].Count == 1 );
         Assert.AreEqual( Graph.Weights[ Graph.GetNode( "A" ) ][ Graph.GetNode( "B" ) ][ 0 ].Weight, 2.0, WEIGHT_TOLERANCE );
      }

      [Test]
      [ExpectedException( typeof( ArgumentException ) )]
      public void UnsuccessfullyAddConnection()
      {
         Graph.AddConnection( "A", "B" ).AddConnection( "A", "B" );
      }

      [Test]
      public void SuccessfullyContainsConnection()
      {
         Graph.AddNode( "A" ).AddNode( "B" ).AddConnection( "A", "B" );
         Assert.IsTrue( Graph.ContainsConnection( "A", "B" ) );
      }

      [Test]
      public void UnsuccessfullyContainsConnection()
      {
         Graph.AddNode( "A" ).AddNode( "B" );
         Assert.IsFalse( Graph.ContainsConnection( "A", "B" ) );
      }

      [Test]
      public void SuccessfullyRemoveConnection()
      {
         Graph.AddNode( "A" ).AddNode( "B" ).AddConnection( "A", "B" ).RemoveConnection( "A", "B" );
         Assert.IsFalse( Graph.ContainsConnection( "A", "B" ) );
      }

      [Test]
      [ExpectedException( typeof( ArgumentException ) )]
      public void UnsuccessfullyRemoveConnection()
      {
         Graph.AddNode( "A" ).AddNode( "B" ).RemoveConnection( "A", "B" );
      }

      protected void BuildAcyclicGraph()
      {
         Graph.AddNode( "A" ).AddNode( "B" ).AddNode( "C" ).AddNode( "D" ).AddNode( "E" ).AddNode( "F" ).AddNode( "G" );

         Graph.AddConnection( "A", "B" ).AddConnection( "A", "C" );
         Graph.AddConnection( "B", "D" ).AddConnection( "B", "E" );
         Graph.AddConnection( "C", "E" ).AddConnection( "C", "F" ).AddConnection( "C", "G" );
         Graph.AddConnection( "D", "E" );
         Graph.AddConnection( "F", "D" );
         Graph.AddConnection( "G", "F" );
      }

      protected void BuildCyclicGraph()
      {
         BuildAcyclicGraph();
         Graph.AddConnection( "D", "A" );
      }

      [Test]
      public void TestTopologicalSort2Partition()
      {
         BuildAcyclicGraph();

         Int32 initialQuantity = Graph.Nodes.Count;

         List<GraphNode<String>> nodes = Graph.TopologicalSort2Partition();

         Assert.AreEqual( initialQuantity, nodes.Count );
         Assert.AreEqual( Graph.GetNode( "A" ), nodes[ 0 ] );
         Assert.AreEqual( Graph.GetNode( "B" ), nodes[ 1 ] );
         Assert.AreEqual( Graph.GetNode( "C" ), nodes[ 2 ] );
         Assert.AreEqual( Graph.GetNode( "G" ), nodes[ 3 ] );
         Assert.AreEqual( Graph.GetNode( "F" ), nodes[ 4 ] );
         Assert.AreEqual( Graph.GetNode( "D" ), nodes[ 5 ] );
         Assert.AreEqual( Graph.GetNode( "E" ), nodes[ 6 ] );
      }

      [Test]
      [ExpectedException( typeof( GraphCycleDetectedException ) )]
      public void TestTopologicalSort2PartitionCycleDetected()
      {
         BuildCyclicGraph();
         Graph.TopologicalSort2Partition();
      }

      [Test]
      public void TestTopologicalSort3Partition()
      {
         BuildAcyclicGraph();

         List<List<GraphNode<String>>> nodes = Graph.TopologicalSort3Partition();

         Assert.IsTrue( ListsAreEqual( new List<GraphNode<String>>() { Graph.GetNode( "A" ) }, nodes[ 0 ] ) );
         Assert.IsTrue( ListsAreEqual( new List<GraphNode<String>>() { Graph.GetNode( "B" ), Graph.GetNode( "C" ) }, nodes[ 1 ] ) );
         Assert.IsTrue( ListsAreEqual( new List<GraphNode<String>>() { Graph.GetNode( "G" ) }, nodes[ 2 ] ) );
         Assert.IsTrue( ListsAreEqual( new List<GraphNode<String>>() { Graph.GetNode( "F" ) }, nodes[ 3 ] ) );
         Assert.IsTrue( ListsAreEqual( new List<GraphNode<String>>() { Graph.GetNode( "D" ) }, nodes[ 4 ] ) );
         Assert.IsTrue( ListsAreEqual( new List<GraphNode<String>>() { Graph.GetNode( "E" ) }, nodes[ 5 ] ) );
      }

      [Test]
      [ExpectedException( typeof( GraphCycleDetectedException ) )]
      public void TestTopologicalSort3PartitionCycleDetected()
      {
         BuildCyclicGraph();
         Graph.TopologicalSort3Partition();
      }

      protected bool ListsAreEqual<T>( List<T> left, List<T> right )
      {
         bool result = left.Count == right.Count;

         if( result )
            for( Int32 i = 0; result && i < left.Count; ++i )
               result = left[ i ].Equals( right[ i ] );

         return result;
      }

      [Test]
      public void TestClone()
      {
         BuildCyclicGraph();

         Graph<String> dup = ( Graph<String> ) Graph.Clone();
         Assert.IsTrue( ListsAreEqual( Graph.Nodes, dup.Nodes ) );

         Assert.AreEqual( dup.Weights.Count, Graph.Weights.Count );
         foreach( GraphNode<String> sourceKeyNode in dup.Weights.Keys )
         {
            Assert.IsTrue( Graph.Weights.ContainsKey( sourceKeyNode ) );
            Assert.AreEqual( dup.Weights[ sourceKeyNode ].Count, Graph.Weights[ sourceKeyNode ].Count );
            foreach( GraphNode<String> destinationKeyNode in dup.Weights[ sourceKeyNode ].Keys )
            {
               Assert.IsTrue( Graph.Weights[ sourceKeyNode ].ContainsKey( destinationKeyNode ) );
               Assert.AreEqual( dup.Weights[ sourceKeyNode ][ destinationKeyNode ].Count, Graph.Weights[ sourceKeyNode ][ destinationKeyNode ].Count );
               for( Int32 i = 0; i < dup.Weights[ sourceKeyNode ][ destinationKeyNode ].Count; ++i )
               {
                  GraphConnection<String> dupConnection = dup.Weights[ sourceKeyNode ][ destinationKeyNode ][ i ];
                  GraphConnection<String> graphConnection = Graph.Weights[ sourceKeyNode ][ destinationKeyNode ][ i ];
                  Assert.AreEqual( dupConnection.Weight, graphConnection.Weight, WEIGHT_TOLERANCE );
               }
            }
         }
      }

      [Test]
      public void TestEquals()
      {
         BuildCyclicGraph();
         Graph<String> dup = ( Graph<String> ) Graph.Clone();
         Assert.AreEqual( Graph, dup );
         dup.AddNode( "I" );
         Assert.AreNotEqual( Graph, dup );
      }

      [Test]
      public void TestHashCode()
      {
         Assert.Pass( "Different instances (even clones) should have different hash codes." );
      }
   }
}
