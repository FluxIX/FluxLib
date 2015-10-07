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
using System.Runtime.Serialization;
using System.Text;

namespace FluxLib.Collections.Generic.Graph
{
   /// <summary>
   /// Generic graph class.
   /// </summary>
   /// <typeparam name="T">Graph node ID type.</typeparam>
   public class Graph<T> : IEquatable<Graph<T>>, ICloneable, IDisposable
   {
      /// <summary>
      /// The default connection weight.
      /// </summary>
      public const Double DEFAULT_EDGE_WEIGHT = 1;

      /// <summary>
      /// The default connection weight tolerance.
      /// </summary>
      public const Double DEFAULT_CONNECTION_WEIGHT_TOLERANCE = .001;
      protected const Int32 NOT_FOUND = -1;

      #region Delegates
      public delegate void NodeCallback( GraphNode<T> node );
      public delegate void ConnectionCallback( GraphNode<T> source, GraphNode<T> destination, Double weight );
      public delegate void SortCallback();
      #endregion

      #region Events
      #region Node Events
      public event NodeCallback BeforeNodeAdded;
      public event NodeCallback AfterNodeAdded;

      public event NodeCallback BeforeNodeRemoved;
      public event NodeCallback AfterNodeRemoved;
      #endregion

      #region Connection Events
      public event ConnectionCallback BeforeConnectionAdded;
      public event ConnectionCallback AfterConnectionAdded;

      public event ConnectionCallback BeforeConnectionRemoved;
      public event ConnectionCallback AfterConnectionRemoved;
      #endregion

      #region Sort Events
      public event SortCallback BeforeSort2Partition;
      public event SortCallback AfterSort2Partition;

      public event SortCallback BeforeSort3Partition;
      public event SortCallback AfterSort3Partition;
      #endregion
      #endregion

      /// <summary>
      /// Constructs a graph with no nodes or connections. The connections have the default connection weight tolerance.
      /// </summary>
      public Graph()
         : this( DEFAULT_CONNECTION_WEIGHT_TOLERANCE )
      {
      }

      /// <summary>
      /// Constructs a graph with no nodes or connections. The connections have the given connection weight tolerance.
      /// </summary>
      /// <param name="connectionWeightTolerance">connection weight tolerance</param>
      public Graph( Double connectionWeightTolerance )
      {
         Nodes = new List<GraphNode<T>>();
         Weights = new Dictionary<GraphNode<T>, Dictionary<GraphNode<T>, List<GraphConnection<T>>>>();
         Sort2PartitionResult = null;
         Sort3PartitionResult = null;
         SetConnectionWeightTolerance( connectionWeightTolerance );

         #region Callback Initialization
         AfterNodeAdded = new NodeCallback( SetSortFlagsNode );
         AfterNodeRemoved = new NodeCallback( SetSortFlagsNode );
         AfterConnectionAdded = new ConnectionCallback( SetSortFlagsConnection );
         AfterConnectionRemoved = new ConnectionCallback( SetSortFlagsConnection );
         #endregion
      }

      /// <summary>
      /// List of the of nodes in the graph.
      /// </summary>
      public List<GraphNode<T>> Nodes
      {
         get;
         protected set;
      }

      /// <summary>
      /// List of the connections' weights in the graph.
      /// </summary>
      public Dictionary<GraphNode<T>, Dictionary<GraphNode<T>, List<GraphConnection<T>>>> Weights
      {
         get;
         protected set;
      }

      /// <summary>
      /// List of results of the two-partition sort.
      /// </summary>
      protected List<GraphNode<T>> Sort2PartitionResult
      {
         get;
         set;
      }

      /// <summary>
      /// List of results of the three-partition sort.
      /// </summary>
      protected List<List<GraphNode<T>>> Sort3PartitionResult
      {
         get;
         set;
      }

      /// <summary>
      /// The graph connections' weight tolerance.
      /// </summary>
      public Double ConnectionWeightTolerance
      {
         get;
         private set;
      }

      /// <summary>
      /// Setter for connection weight tolerance. Does range validation.
      /// </summary>
      /// <param name="value">new weight tolerance</param>
      protected void SetConnectionWeightTolerance( Double value )
      {
         if( value >= 0.0 )
            ConnectionWeightTolerance = value;
         else
            throw new ArgumentOutOfRangeException( "value", value, "Connection weight tolerance must be greater than or equal to zero." );
      }

      /// <summary>
      /// The number of connections in the graph.
      /// </summary>
      public Int32 ConnectionCount
      {
         get
         {
            Int32 result = 0;

            foreach( GraphNode<T> sourceKey in Weights.Keys )
               foreach( GraphNode<T> destinationKey in Weights[ sourceKey ].Keys )
                  result += Weights[ sourceKey ][ destinationKey ].Count;

            return result;
         }
      }

      /// <summary>
      /// Determines if a node with the given ID is in the graph.
      /// </summary>
      /// <param name="id">target node ID</param>
      /// <returns>true if a node with the given ID exists in the graph, false otherwise.</returns>
      protected bool ContainsNodeId( T id )
      {
         if( id != null )
         {
            bool result = false;

            for( Int32 i = 0; !result && i < Nodes.Count; ++i )
               result = Nodes[ i ].Id.Equals( id );

            return result;
         }
         else
            throw new ArgumentNullException( "id", "Node id cannot be null." );
      }

      /// <summary>
      /// Gets the node index of the node with the given ID.
      /// </summary>
      /// <param name="id">target node ID</param>
      /// <returns>The index of the node with the given ID, NOT_FOUND otherwise.</returns>
      protected Int32 LocateNode( T id )
      {
         Int32 result = NOT_FOUND;

         for( Int32 i = 0; result == NOT_FOUND && i < Nodes.Count; ++i )
            if( Nodes[ i ].Id.Equals( id ) )
               result = i;

         return result;
      }

      /// <summary>
      /// Retrieves the node with the given ID.
      /// </summary>
      /// <param name="id">target node ID</param>
      /// <returns>GraphNode with the given ID, null otherwise.</returns>
      public GraphNode<T> GetNode( T id )
      {
         GraphNode<T> result = null;

         Int32 index = LocateNode( id );
         if( index != NOT_FOUND )
            result = Nodes[ index ];

         return result;
      }

      /// <summary>
      /// Determines if a node with the given node ID exists in the graph.
      /// </summary>
      /// <param name="id">target node ID</param>
      /// <returns>true if a node with the given ID exists in the graph, false otherwise.</returns>
      public bool ContainsNode( T id )
      {
         if( id == null )
            throw new ArgumentNullException( "id", "Id cannot be null." );
         else
            return LocateNode( id ) != NOT_FOUND;
      }

      /// <summary>
      /// Determines if the given node exists in the graph.
      /// </summary>
      /// <param name="node">target node</param>
      /// <returns>true if the graph contains the given node, false otherwise</returns>
      public bool ContainsNode( GraphNode<T> node )
      {
         return Nodes.Contains( node );
      }

      /// <summary>
      /// Adds a new node with the given ID to graph.
      /// </summary>
      /// <param name="nodeId">new node ID</param>
      /// <returns>graph object</returns>
      public Graph<T> AddNode( T nodeId )
      {
         if( ContainsNodeId( nodeId ) )
            throw new ArgumentException( "Node id already present. Node ids must be unique.", "nodeId" );
         else
         {
            GraphNode<T> newNode = new GraphNode<T>( nodeId );

            if( BeforeNodeAdded != null )
               BeforeNodeAdded.Invoke( newNode );

            Nodes.Add( newNode );

            if( AfterNodeAdded != null )
               AfterNodeAdded.Invoke( newNode );
         }

         return this;
      }

      /// <summary>
      /// Removes node with the given ID from the graph.
      /// </summary>
      /// <param name="nodeId">node ID of the node to remove</param>
      /// <returns>graph object</returns>
      public Graph<T> RemoveNode( T nodeId )
      {
         if( nodeId == null )
            throw new ArgumentNullException( "nodeId", "Node Id cannot be null." );
         else
         {
            GraphNode<T> node = GetNode( nodeId );
            if( node != null )
               return RemoveNode( node );
            else
               throw new ArgumentException( "Node with nodeId not found to remove." );
         }
      }

      /// <summary>
      /// Removes the given node from the graph.
      /// </summary>
      /// <param name="node">node to remove</param>
      /// <returns>graph object</returns>
      public Graph<T> RemoveNode( GraphNode<T> node )
      {
         if( node == null )
            throw new ArgumentNullException( "node", "Node to remove cannot be null." );
         else if( Nodes.Contains( node ) )
         {
            if( BeforeNodeRemoved != null )
               BeforeNodeRemoved.Invoke( node );

            foreach( GraphNode<T> predecessor in node.Precedessors )
               RemoveConnection( predecessor, node );
            foreach( GraphNode<T> successor in node.Successors )
               RemoveConnection( node, successor );

            Nodes.Remove( node );

            if( AfterNodeRemoved != null )
               AfterNodeRemoved.Invoke( node );
         }
         else
            throw new ArgumentException( "Node list does not contain the node to remove." );

         return this;
      }

      /// <summary>
      /// Determines if a connection exists between the node with the given source ID and node with the given destination ID.
      /// </summary>
      /// <param name="sourceId">source node ID</param>
      /// <param name="destinationId">destination node ID</param>
      /// <returns>true if a connection exists from source to destination, false otherwise</returns>
      public bool ContainsConnection( T sourceId, T destinationId )
      {
         if( sourceId == null || destinationId == null )
            throw new ArgumentNullException( "Connections cannot have null endpoints." );
         else
         {
            bool result = false;

            GraphNode<T> destination = GetNode( destinationId );
            if( destination != null )
               result = ContainsConnection( sourceId, destination );

            return result;
         }
      }

      /// <summary>
      /// Determines if a connection exists between the source node and node with the given destination ID.
      /// </summary>
      /// <param name="source">source node</param>
      /// <param name="destinationId">destination node ID</param>
      /// <returns>true if a connection exists from source to destination, false otherwise</returns>
      public bool ContainsConnection( GraphNode<T> source, T destinationId )
      {
         if( source == null || destinationId == null )
            throw new ArgumentNullException( "Connections cannot have null endpoints." );
         else
         {
            bool result = false;

            GraphNode<T> destination = GetNode( destinationId );
            if( destination != null )
               result = ContainsConnection( source, destination );

            return result;
         }
      }

      /// <summary>
      /// Determines if a connection exists between the node with the given source ID and destination node.
      /// </summary>
      /// <param name="sourceId">source node ID</param>
      /// <param name="destination">destination node</param>
      /// <returns>true if a connection exists from source to destination, false otherwise</returns>
      public bool ContainsConnection( T sourceId, GraphNode<T> destination )
      {
         if( sourceId == null || destination == null )
            throw new ArgumentNullException( "Connections cannot have null endpoints." );
         else
         {
            bool result = false;

            GraphNode<T> source = GetNode( sourceId );
            if( source != null )
               result = ContainsConnection( source, destination );

            return result;
         }
      }

      /// <summary>
      /// Determines if a connection exists between the source and destination nodes.
      /// </summary>
      /// <param name="source">source node</param>
      /// <param name="destination">destination node</param>
      /// <returns>true if a connection exists from source to destination, false otherwise</returns>
      public bool ContainsConnection( GraphNode<T> source, GraphNode<T> destination )
      {
         if( source == null || destination == null )
            throw new ArgumentNullException( "Connections cannot have null endpoints." );
         else
            return Weights.ContainsKey( source ) && Weights[ source ].ContainsKey( destination ) && Weights[ source ][ destination ].Count > 0;
      }

      /// <summary>
      /// Determines if a connection exists between the node with the given source ID and node with the given destination ID.
      /// </summary>
      /// <param name="sourceId">source node ID</param>
      /// <param name="destinationId">destination node ID</param>
      /// <param name="weight">connection weight</param>
      /// <returns>true if a connection with the given weight exists from source to destination, false otherwise</returns>
      public bool ContainsConnection( T sourceId, T destinationId, Double weight )
      {
         if( sourceId == null || destinationId == null )
            throw new ArgumentNullException( "Connections cannot have null endpoints." );
         else
         {
            bool result = false;

            GraphNode<T> destination = GetNode( destinationId );
            if( destination != null )
               result = ContainsConnection( sourceId, destination, weight );

            return result;
         }
      }

      /// <summary>
      /// Determines if a connection exists between the source node and node with the given destination ID.
      /// </summary>
      /// <param name="source">source node</param>
      /// <param name="destinationId">destination node ID</param>
      /// <param name="weight">connection weight</param>
      /// <returns>true if a connection with the given weight exists from source to destination, false otherwise</returns>
      public bool ContainsConnection( GraphNode<T> source, T destinationId, Double weight )
      {
         if( source == null || destinationId == null )
            throw new ArgumentNullException( "Connections cannot have null endpoints." );
         else
         {
            bool result = false;

            GraphNode<T> destination = GetNode( destinationId );
            if( destination != null )
               result = ContainsConnection( source, destination, weight );

            return result;
         }
      }

      /// <summary>
      /// Determines if a connection exists between the node with the given source ID and destination node.
      /// </summary>
      /// <param name="sourceId">source node ID</param>
      /// <param name="destination">destination node</param>
      /// <param name="weight">connection weight</param>
      /// <returns>true if a connection with the given weight exists from source to destination, false otherwise</returns>
      public bool ContainsConnection( T sourceId, GraphNode<T> destination, Double weight )
      {
         if( sourceId == null || destination == null )
            throw new ArgumentNullException( "Connections cannot have null endpoints." );
         else
         {
            bool result = false;

            GraphNode<T> source = GetNode( sourceId );
            if( destination != null )
               result = ContainsConnection( source, destination, weight );

            return result;
         }
      }

      /// <summary>
      /// Determines if a connection exists between the source and destination nodes with the given weight.
      /// </summary>
      /// <param name="source">source node</param>
      /// <param name="destination">destination node</param>
      /// <param name="weight">connection weight</param>
      /// <returns>true if a connection with the given weight exists from source to destination, false otherwise</returns>
      public bool ContainsConnection( GraphNode<T> source, GraphNode<T> destination, Double weight )
      {
         if( source == null || destination == null )
            throw new ArgumentNullException( "Connections cannot have null endpoints." );
         else
         {
            bool result = Weights.ContainsKey( source ) && Weights[ source ].ContainsKey( destination ) && Weights[ source ][ destination ].Count > 0;

            if( result )
            {
               result = false;
               for( Int32 i = 0; !result && i < Weights[ source ][ destination ].Count; ++i )
                  result = Math.Abs( Weights[ source ][ destination ][ i ].Weight - weight ) <= ConnectionWeightTolerance; // Equals comparison with tolerance.
            }

            return result;
         }
      }

      /// <summary>
      /// Adds a new connection from the node with the given source ID to the node with the given destination ID. Node connection endpoint pairs are unique.
      /// </summary>
      /// <param name="sourceId">source node ID</param>
      /// <param name="destinationId">destination node ID</param>
      /// <returns>graph object</returns>
      public Graph<T> AddConnection( T sourceId, T destinationId )
      {
         if( sourceId == null || destinationId == null )
            throw new ArgumentNullException( "Connection endpoints cannot be null." );
         else
         {
            GraphNode<T> source = GetNode( sourceId );
            if( source != null )
               return AddConnection( source, destinationId );
            else
               throw new ArgumentException( "Node with id (" + sourceId + ") does not exist." );
         }
      }

      /// <summary>
      /// Adds a new connection from the source node to the node with the given destination ID. Node connection endpoint pairs are unique.
      /// </summary>
      /// <param name="source">source node</param>
      /// <param name="destinationId">destination node ID</param>
      /// <returns>graph object</returns>
      public Graph<T> AddConnection( GraphNode<T> source, T destinationId )
      {
         if( source == null || destinationId == null )
            throw new ArgumentNullException( "Connection endpoints cannot be null." );
         else
         {
            GraphNode<T> destination = GetNode( destinationId );
            if( destination != null )
               return AddConnection( source, destination );
            else
               throw new ArgumentException( "Node with id (" + destinationId + ") does not exist." );
         }
      }

      /// <summary>
      /// Adds a new connection from the node with the given source ID to the destination node with the given weight. Node connection endpoint pairs are unique.
      /// </summary>
      /// <param name="sourceId">source node ID</param>
      /// <param name="destination">destination node</param>
      /// <returns>graph object</returns>
      public Graph<T> AddConnection( T sourceId, GraphNode<T> destination )
      {
         if( sourceId == null || destination == null )
            throw new ArgumentNullException( "Connection endpoints cannot be null." );
         else
         {
            GraphNode<T> source = GetNode( sourceId );
            if( source != null )
               return AddConnection( source, destination );
            else
               throw new ArgumentException( "Node with id (" + sourceId + ") does not exist." );
         }
      }

      /// <summary>
      /// Adds a new connection from the source node to the destination node with the given weight. Node connection endpoint pairs are unique.
      /// </summary>
      /// <param name="sourceId">source node ID</param>
      /// <param name="destination">destination node</param>
      /// <returns>graph object</returns>
      public Graph<T> AddConnection( GraphNode<T> source, GraphNode<T> destination )
      {
         if( source == null || destination == null )
            throw new ArgumentNullException( "Connection endpoints cannot be null." );
         else
            return AddConnection( source, destination, DEFAULT_EDGE_WEIGHT );
      }

      /// <summary>
      /// Adds a new connection from the node with the given source ID to the node with the given destination ID with the given weight. Node connection endpoint pairs are unique.
      /// </summary>
      /// <param name="sourceId">source node ID</param>
      /// <param name="destinationId">destination node ID</param>
      /// <param name="weight">connection weight</param>
      /// <returns>graph object</returns>
      public Graph<T> AddConnection( T sourceId, T destinationId, Double weight )
      {
         if( sourceId == null || destinationId == null )
            throw new ArgumentNullException( "Connection endpoints cannot be null." );
         else
         {
            GraphNode<T> source = GetNode( sourceId );
            if( source != null )
               return AddConnection( source, destinationId, weight );
            else
               throw new ArgumentException( "Node with id (" + sourceId + ") does not exist." );
         }
      }

      /// <summary>
      /// Adds a new connection from the source node to the node with the given destination ID with the given weight. Node connection endpoint pairs are unique.
      /// </summary>
      /// <param name="sourceId">source node ID</param>
      /// <param name="destinationId">destination node ID</param>
      /// <param name="weight">connection weight</param>
      /// <returns>graph object</returns>
      public Graph<T> AddConnection( GraphNode<T> source, T destinationId, Double weight )
      {
         if( source == null || destinationId == null )
            throw new ArgumentNullException( "Connection endpoints cannot be null." );
         else
         {
            GraphNode<T> destination = GetNode( destinationId );
            if( destination != null )
               return AddConnection( source, destination, weight );
            else
               throw new ArgumentException( "Node with id (" + destinationId + ") does not exist." );
         }
      }

      /// <summary>
      /// Adds a new connection from the node with the given source ID to the destination node with the given weight. Node connection endpoint pairs are unique.
      /// </summary>
      /// <param name="sourceId">source node ID</param>
      /// <param name="destination">destination node</param>
      /// <param name="weight">connection weight</param>
      /// <returns>graph object</returns>
      public Graph<T> AddConnection( T sourceId, GraphNode<T> destination, Double weight )
      {
         if( sourceId == null || destination == null )
            throw new ArgumentNullException( "Connection endpoints cannot be null." );
         else
         {
            GraphNode<T> source = GetNode( sourceId );
            if( source != null )
               return AddConnection( source, destination, weight );
            else
               throw new ArgumentException( "Node with id (" + sourceId + ") does not exist." );
         }
      }

      /// <summary>
      /// Adds a new connection from the source node to the destination node with the given weight. Node connection endpoint pairs are unique.
      /// </summary>
      /// <param name="sourceId">source node ID</param>
      /// <param name="destination">destination node</param>
      /// <param name="weight">connection weight</param>
      /// <returns>graph object</returns>
      public Graph<T> AddConnection( GraphNode<T> source, GraphNode<T> destination, Double weight )
      {
         if( source == null || destination == null )
            throw new ArgumentNullException( "Connection nodes cannot be null." );
         else if( !ContainsNode( source ) || !ContainsNode( destination ) )
            throw new ArgumentException( "Not all nodes present in graph." );
         else
         {
            if( BeforeConnectionAdded != null )
               BeforeConnectionAdded.Invoke( source, destination, weight );

            if( !Weights.ContainsKey( source ) )
               Weights.Add( source, new Dictionary<GraphNode<T>, List<GraphConnection<T>>>() );
            if( !Weights[ source ].ContainsKey( destination ) )
               Weights[ source ].Add( destination, new List<GraphConnection<T>>() );
            Weights[ source ][ destination ].Add( new GraphConnection<T>( source, destination, weight, ConnectionWeightTolerance ) );

            if( !source.Successors.Contains( destination ) )
               source.Successors.Add( destination );
            if( !destination.Precedessors.Contains( source ) )
               destination.Precedessors.Add( source );

            if( AfterConnectionAdded != null )
               AfterConnectionAdded.Invoke( source, destination, weight );
         }

         return this;
      }

      /// <summary>
      /// Removes a connection between the node with the given source ID and the node with the given destination ID.
      /// </summary>
      /// <param name="sourceId">source node ID</param>
      /// <param name="destinationId">destination node ID</param>
      /// <returns>graph object</returns>
      public Graph<T> RemoveConnection( T sourceId, T destinationId )
      {
         if( sourceId == null || destinationId == null )
            throw new ArgumentNullException( "Connection endpoints cannot be null." );
         else
         {
            GraphNode<T> source = GetNode( sourceId );
            if( source != null )
               return RemoveConnection( source, destinationId );
            else
               throw new ArgumentException( "Node with id (" + sourceId + ") does not exist." );
         }
      }

      /// <summary>
      /// Removes a connection between the source node and the node with the given destination ID.
      /// </summary>
      /// <param name="source">source node</param>
      /// <param name="destinationId">destination node ID</param>
      /// <returns>graph object</returns>
      public Graph<T> RemoveConnection( GraphNode<T> source, T destinationId )
      {
         if( source == null || destinationId == null )
            throw new ArgumentNullException( "Connection endpoints cannot be null." );
         else
         {
            GraphNode<T> destination = GetNode( destinationId );
            if( destination != null )
               return RemoveConnection( source, destination );
            else
               throw new ArgumentException( "Node with id (" + destinationId + ") does not exist." );
         }
      }

      /// <summary>
      /// Removes the connection between the node with the given source ID and the destination node.
      /// </summary>
      /// <param name="sourceId">source node ID</param>
      /// <param name="destination">destination node</param>
      /// <returns>graph object</returns>
      public Graph<T> RemoveConnection( T sourceId, GraphNode<T> destination )
      {
         if( sourceId == null || destination == null )
            throw new ArgumentNullException( "Connection endpoints cannot be null." );
         else
         {
            GraphNode<T> source = GetNode( sourceId );
            if( source != null )
               return RemoveConnection( source, destination );
            else
               throw new ArgumentException( "Node with id (" + sourceId + ") does not exist." );
         }
      }

      /// <summary>
      /// Removes a connection between the source node and the destination node.
      /// </summary>
      /// <param name="source">source node</param>
      /// <param name="destination">destination node</param>
      /// <returns>graph object</returns>
      public Graph<T> RemoveConnection( GraphNode<T> source, GraphNode<T> destination )
      {
         if( source == null || destination == null )
            throw new ArgumentNullException( "Connection nodes cannot be null." );
         else if( !ContainsNode( source ) || !ContainsNode( destination ) )
            throw new ArgumentException( "Not all nodes present in graph." );
         // Since connection data is stored in both the Weights data and the nodes' predecessors and successors, checks must be done to ensure all connection data is present.
         else if( !ContainsConnection( source, destination ) )
            throw new ArgumentException( "No connection exists between the source and destination nodes." );
         else if( !destination.Precedessors.Contains( source ) )
            throw new ArgumentException( "Source node is not a predecessor of the destination node." );
         else if( !source.Successors.Contains( destination ) )
            throw new ArgumentException( "Destination node is not a successor of the source node." );
         else
            RemoveConnection( source, destination, 0 );

         return this;
      }

      /// <summary>
      /// Removes a connection between the node with the given source ID and the node with the given destination ID with the given weight.
      /// </summary>
      /// <param name="sourceId">source node ID</param>
      /// <param name="destinationId">destination node ID</param>
      /// <param name="weight">connection weight</param>
      /// <returns>graph object</returns>
      public Graph<T> RemoveConnection( T sourceId, T destinationId, Double weight )
      {
         if( sourceId == null || destinationId == null )
            throw new ArgumentNullException( "Connection endpoints cannot be null." );
         else
         {
            GraphNode<T> source = GetNode( sourceId );
            if( source != null )
               return RemoveConnection( source, destinationId, weight );
            else
               throw new ArgumentException( "Node with id (" + sourceId + ") does not exist." );
         }
      }

      /// <summary>
      /// Removes a connection between the source node and the node with the given destination ID with the given weight.
      /// </summary>
      /// <param name="source">source node</param>
      /// <param name="destinationId">destination node ID</param>
      /// <param name="weight">connection weight</param>
      /// <returns>graph object</returns>
      public Graph<T> RemoveConnection( GraphNode<T> source, T destinationId, Double weight )
      {
         if( source == null || destinationId == null )
            throw new ArgumentNullException( "Connection endpoints cannot be null." );
         else
         {
            GraphNode<T> destination = GetNode( destinationId );
            if( destination != null )
               return RemoveConnection( source, destination, weight );
            else
               throw new ArgumentException( "Node with id (" + destinationId + ") does not exist." );
         }
      }

      /// <summary>
      /// Removes the connection between the node with the given source ID and the destination node with the given weight.
      /// </summary>
      /// <param name="sourceId">source node ID</param>
      /// <param name="destination">destination node</param>
      /// <param name="weight">connection weight</param>
      /// <returns>graph object</returns>
      public Graph<T> RemoveConnection( T sourceId, GraphNode<T> destination, Double weight )
      {
         if( sourceId == null || destination == null )
            throw new ArgumentNullException( "Connection endpoints cannot be null." );
         else
         {
            GraphNode<T> source = GetNode( sourceId );
            if( source != null )
               return RemoveConnection( source, destination, weight );
            else
               throw new ArgumentException( "Node with id (" + sourceId + ") does not exist." );
         }
      }

      /// <summary>
      /// Removes a connection between the source node and the destination node with the given weight.
      /// </summary>
      /// <param name="source">source node</param>
      /// <param name="destination">destination node</param>
      /// <param name="weight">connection weight</param>
      /// <returns>graph object</returns>
      public Graph<T> RemoveConnection( GraphNode<T> source, GraphNode<T> destination, Double weight )
      {
         if( source == null || destination == null )
            throw new ArgumentNullException( "Connection nodes cannot be null." );
         else if( !ContainsNode( source ) || !ContainsNode( destination ) )
            throw new ArgumentException( "Not all nodes present in graph." );
         // Since connection data is stored in both the Weights data and the nodes' predecessors and successors, checks must be done to ensure all connection data is present.
         else if( !ContainsConnection( source, destination ) )
            throw new ArgumentException( "No connection exists between the source and destination nodes." );
         else if( !destination.Precedessors.Contains( source ) )
            throw new ArgumentException( "Source node is not a predecessor of the destination node." );
         else if( !source.Successors.Contains( destination ) )
            throw new ArgumentException( "Destination node is not a successor of the source node." );
         else
         {
            Int32 index = -1;
            for( Int32 i = 0; i < Weights[ source ][ destination ].Count; ++i )
               if( Math.Abs( Weights[ source ][ destination ][ i ].Weight - weight ) <= ConnectionWeightTolerance ) // Equals comparison with tolerance.
                  index = i;
            RemoveConnection( source, destination, index );
         }

         return this;
      }

      /// <summary>
      /// Removes all connections between the node with the given source ID and the node with the given destination ID.
      /// </summary>
      /// <param name="sourceId">source node ID</param>
      /// <param name="destinationId">destination node ID</param>
      /// <returns>graph object</returns>
      public Graph<T> RemoveAllConnections( T sourceId, T destinationId )
      {
         if( sourceId == null || destinationId == null )
            throw new ArgumentNullException( "Connection endpoints cannot be null." );
         else
         {
            GraphNode<T> source = GetNode( sourceId );
            if( source != null )
               return RemoveAllConnections( source, destinationId );
            else
               throw new ArgumentException( "Node with id (" + sourceId + ") does not exist." );
         }
      }

      /// <summary>
      /// Removes all connections between the source node and the node with the given destination ID.
      /// </summary>
      /// <param name="source">source node</param>
      /// <param name="destinationId">destination node ID</param>
      /// <returns>graph object</returns>
      public Graph<T> RemoveAllConnections( GraphNode<T> source, T destinationId )
      {
         if( source == null || destinationId == null )
            throw new ArgumentNullException( "Connection endpoints cannot be null." );
         else
         {
            GraphNode<T> destination = GetNode( destinationId );
            if( destination != null )
               return RemoveAllConnections( source, destination );
            else
               throw new ArgumentException( "Node with id (" + destinationId + ") does not exist." );
         }
      }

      /// <summary>
      /// Removes all connections between the node with the given source ID and the destination node.
      /// </summary>
      /// <param name="sourceId">source node ID</param>
      /// <param name="destination">destination node</param>
      /// <returns>graph object</returns>
      public Graph<T> RemoveAllConnections( T sourceId, GraphNode<T> destination )
      {
         if( sourceId == null || destination == null )
            throw new ArgumentNullException( "Connection endpoints cannot be null." );
         else
         {
            GraphNode<T> source = GetNode( sourceId );
            if( source != null )
               return RemoveAllConnections( source, destination );
            else
               throw new ArgumentException( "Node with id (" + sourceId + ") does not exist." );
         }
      }

      /// <summary>
      /// Removes all connections between the source node and the destination node.
      /// </summary>
      /// <param name="source">source node</param>
      /// <param name="destination">destination node</param>
      /// <returns>graph object</returns>
      public Graph<T> RemoveAllConnections( GraphNode<T> source, GraphNode<T> destination )
      {
         if( source == null || destination == null )
            throw new ArgumentNullException( "Connection nodes cannot be null." );
         else if( !ContainsNode( source ) || !ContainsNode( destination ) )
            throw new ArgumentException( "Not all nodes present in graph." );
         // Since connection data is stored in both the Weights data and the nodes' predecessors and successors, checks must be done to ensure all connection data is present.
         else if( !ContainsConnection( source, destination ) )
            throw new ArgumentException( "No connection exists between the source and destination nodes." );
         else if( !destination.Precedessors.Contains( source ) )
            throw new ArgumentException( "Source node is not a predecessor of the destination node." );
         else if( !source.Successors.Contains( destination ) )
            throw new ArgumentException( "Destination node is not a successor of the source node." );
         else
            for( Int32 i = Weights[ source ][ destination ].Count - 1; i >= 0; --i )
               RemoveConnection( source, destination, i );

         return this;
      }

      /// <summary>
      /// Removes the connection between the source node and the destination at the given index into the connection weights list.
      /// </summary>
      /// <param name="source">source node</param>
      /// <param name="destination">destination node</param>
      /// <param name="connectionIndex">connection index into the weights list</param>
      protected void RemoveConnection( GraphNode<T> source, GraphNode<T> destination, Int32 connectionIndex )
      {
         Double weight = Weights[ source ][ destination ][ connectionIndex ].Weight;

         if( BeforeConnectionRemoved != null )
            BeforeConnectionRemoved.Invoke( source, destination, weight );

         Weights[ source ][ destination ].RemoveAt( connectionIndex );

         if( Weights[ source ][ destination ].Count == 0 )
         {
            source.Successors.Remove( destination );
            destination.Precedessors.Remove( source );
         }

         if( AfterConnectionRemoved != null )
            AfterConnectionRemoved.Invoke( source, destination, weight );
      }

      /// <summary>
      /// Removes all nodes and all connections from the graph. It does not unregister the registered delegates.
      /// </summary>
      public void Clear()
      {
         foreach( GraphNode<T> sourceKey in Weights.Keys )
            Weights[ sourceKey ].Clear();
         Weights.Clear();

         if( Sort2PartitionResult != null )
            Sort2PartitionResult.Clear();

         if( Sort3PartitionResult != null )
            Sort3PartitionResult.Clear();

         Nodes.Clear();
      }

      /// <summary>
      /// Topologically sorts the graph, implicitly preserving equivalent priorities.
      /// </summary>
      /// <returns>List of node in ascending order (least dependent to most dependent).</returns>
      /// <remarks>Adapted from https://en.wikipedia.org/wiki/Topological_sorting</remarks>
      public List<GraphNode<T>> TopologicalSort2Partition()
      {
         if( BeforeSort2Partition != null )
            BeforeSort2Partition.Invoke();

         if( Sort2PartitionResult == null )
         {
            Sort2PartitionResult = new List<GraphNode<T>>( Nodes.Count );

            Queue<GraphNode<T>> terminalNodes = new Queue<GraphNode<T>>();
            foreach( GraphNode<T> node in Nodes )
               if( node.Precedessors.Count == 0 )
                  terminalNodes.Enqueue( node );

            while( terminalNodes.Count > 0 )
            {
               GraphNode<T> currentNode = terminalNodes.Dequeue();
               Sort2PartitionResult.Add( currentNode );

               foreach( GraphNode<T> node in currentNode.Successors )
                  if( IsTargetSubsetOfGroup( node.Precedessors, Sort2PartitionResult ) )
                     terminalNodes.Enqueue( node );
            }

            if( Sort2PartitionResult.Count < Nodes.Count )
               throw new GraphCycleDetectedException( "Cycle detected in graph." );
         }

         if( AfterSort2Partition != null )
            AfterSort2Partition.Invoke();

         return Sort2PartitionResult;
      }

      /// <summary>
      /// Topologically sorts the graph, explicitly preserving equivalent priorities.
      /// </summary>
      /// <returns>Linked-list of node groupings in ascending order (least dependent to most dependent. Each node in a group is independent.</returns>
      /// <remarks>Adapted from https://en.wikipedia.org/wiki/Topological_sorting</remarks>
      public List<List<GraphNode<T>>> TopologicalSort3Partition()
      {
         if( BeforeSort3Partition != null )
            BeforeSort3Partition.Invoke();

         if( Sort3PartitionResult == null )
         {
            Sort3PartitionResult = new List<List<GraphNode<T>>>();

            List<GraphNode<T>> processedNodes = new List<GraphNode<T>>( Nodes.Count );
            Queue<GraphNode<T>> terminalNodes = new Queue<GraphNode<T>>();

            foreach( GraphNode<T> node in Nodes )
               if( node.Precedessors.Count == 0 )
                  terminalNodes.Enqueue( node );

            List<GraphNode<T>> bucket = new List<GraphNode<T>>( terminalNodes.Count );

            GraphNode<T> currentNode;
            while( terminalNodes.Count > 0 )
            {
               currentNode = terminalNodes.Dequeue();
               bucket.Add( currentNode );
               processedNodes.Add( currentNode );

               foreach( GraphNode<T> successor in currentNode.Successors )
                  if( IsTargetSubsetOfGroup( successor.Precedessors, processedNodes ) )
                     terminalNodes.Enqueue( successor );

               if( bucket.Count == bucket.Capacity ) // Are all items of target priority in the bucket?
               {
                  Sort3PartitionResult.Add( bucket );
                  if( terminalNodes.Count > 0 )
                     bucket = new List<GraphNode<T>>( terminalNodes.Count );
               }
            }

            if( processedNodes.Count < Nodes.Count )
               throw new GraphCycleDetectedException( "Cycle detected in graph." );
         }

         if( AfterSort3Partition != null )
            AfterSort3Partition.Invoke();

         return Sort3PartitionResult;
      }

      /// <summary>
      /// Callback implementation invoked when the node list changes. Ensures sorting will be done.
      /// </summary>
      /// <param name="node">node which altered the node list</param>
      protected void SetSortFlagsNode( GraphNode<T> node )
      {
         Sort2PartitionResult = null;
         Sort3PartitionResult = null;
      }

      /// <summary>
      /// Callback implementation invoked when the connection list changes. Ensures sorting will be done.
      /// </summary>
      /// <param name="source">source node</param>
      /// <param name="destination">destination node</param>
      /// <param name="weight">connection weight</param>
      protected void SetSortFlagsConnection( GraphNode<T> source, GraphNode<T> destination, Double weight )
      {
         Sort2PartitionResult = null;
         Sort3PartitionResult = null;
      }

      /// <summary>
      /// Determines if all of the items of the target are in the group.
      /// </summary>
      /// <typeparam name="U">list item type</typeparam>
      /// <param name="target">list of items to determine subset status of</param>
      /// <param name="group">reference list of items</param>
      /// <returns>true if all of the items in target are in group, false otherwise.</returns>
      protected bool IsTargetSubsetOfGroup<U>( List<U> target, List<U> group )
      {
         bool result = true;

         for( Int32 i = 0; result && i < target.Count; ++i )
            result = group.Contains( target[ i ] );

         return result;
      }

      #region Object Overrides
      public override int GetHashCode()
      {
         int result = base.GetHashCode();

         result ^= Nodes.GetHashCode();
         result ^= Weights.GetHashCode();

         return result;
      }

      public override bool Equals( Object obj )
      {
         Graph<T> g = obj as Graph<T>;

         bool result;

         if( result = g != null )
            result = Equals( g );

         return result;
      }
      #endregion

      #region Interface Implementations
      #region ICloneable Members
      /// <summary>
      /// Creates and returns a deep-copy of the current graph.
      /// </summary>
      /// <returns>the deep-copy of the graph</returns>
      public Object Clone()
      {
         Graph<T> result = new Graph<T>( ConnectionWeightTolerance );

         foreach( GraphNode<T> node in Nodes )
            result.Nodes.Add( new GraphNode<T>( node.Id ) );
         foreach( GraphNode<T> node in Nodes )
         {
            foreach( GraphNode<T> predecessor in node.Precedessors )
               result.GetNode( node.Id ).Precedessors.Add( result.GetNode( predecessor.Id ) );
            foreach( GraphNode<T> successor in node.Successors )
               result.GetNode( node.Id ).Successors.Add( result.GetNode( successor.Id ) );
         }

         foreach( GraphNode<T> sourceKey in Weights.Keys )
         {
            result.Weights.Add( sourceKey, new Dictionary<GraphNode<T>, List<GraphConnection<T>>>() );
            foreach( GraphNode<T> destinationKey in Weights[ sourceKey ].Keys )
            {
               result.Weights[ sourceKey ].Add( destinationKey, new List<GraphConnection<T>>() );
               foreach( GraphConnection<T> connection in Weights[ sourceKey ][ destinationKey ] )
                  result.Weights[ sourceKey ][ destinationKey ].Add( new GraphConnection<T>( result.GetNode( connection.Source.Id ), result.GetNode( connection.Destination.Id ), connection.Weight, result.ConnectionWeightTolerance ) );
            }
         }

         if( Sort2PartitionResult != null )
         {
            result.Sort2PartitionResult = new List<GraphNode<T>>( Sort2PartitionResult.Count );
            foreach( GraphNode<T> node in Sort2PartitionResult )
               result.Sort2PartitionResult.Add( result.GetNode( node.Id ) );
         }

         if( Sort3PartitionResult != null )
         {
            result.Sort3PartitionResult = new List<List<GraphNode<T>>>( Sort3PartitionResult );
            foreach( List<GraphNode<T>> nodes in Sort3PartitionResult )
            {
               List<GraphNode<T>> list = new List<GraphNode<T>>( nodes.Count );

               foreach( GraphNode<T> node in nodes )
                  list.Add( result.GetNode( node.Id ) );

               result.Sort3PartitionResult.Add( list );
            }
         }

         return result;
      }
      #endregion

      #region IDisposable Members
      public void Dispose()
      {
         Clear();
      }
      #endregion

      #region IEquatable<Graph<T>> Members
      /// <summary>
      /// Determines if the current graph is equal to the given graph. The weight connection order does matter.
      /// </summary>
      /// <param name="other">the other graph</param>
      /// <returns>true if the current graph is equal to the given graph, false otherwise.</returns>
      public bool Equals( Graph<T> other )
      {
         bool result = Object.ReferenceEquals( this, other );

         if( !result && Nodes.Count == other.Nodes.Count && Weights.Count == other.Weights.Count )
         {
            result = true;
            for( Int32 i = 0; result && i < Nodes.Count; ++i )
               result = Nodes[ i ].Equals( other.Nodes[ i ] );

            if( result )
            {
               // Note: Dictionary KeyCollection objects not indexable.
               foreach( GraphNode<T> sourceKeyNode in Weights.Keys )
               {
                  if( result = other.Weights.ContainsKey( sourceKeyNode ) )
                     if( result = Weights[ sourceKeyNode ].Count == other.Weights[ sourceKeyNode ].Count )
                        foreach( GraphNode<T> destinationKeyNode in Weights[ sourceKeyNode ].Keys )
                           if( !( result = other.Weights[ sourceKeyNode ].ContainsKey( destinationKeyNode ) ) )
                              break;

                  if( !result )
                     break;
               }

               if( result )
               {
                  foreach( GraphNode<T> sourceKeyNode in Weights.Keys )
                  {
                     foreach( GraphNode<T> destinationKeyNode in Weights[ sourceKeyNode ].Keys )
                     {
                        List<GraphConnection<T>> theseConnections = Weights[ sourceKeyNode ][ destinationKeyNode ];
                        List<GraphConnection<T>> otherConnections = other.Weights[ sourceKeyNode ][ destinationKeyNode ];

                        if( result = theseConnections.Count == otherConnections.Count )
                           for( Int32 i = 0; result && i < theseConnections.Count; ++i )
                              result = theseConnections[ i ].Equals( otherConnections[ i ] );

                        if( !result )
                           break;
                     }

                     if( !result )
                        break;
                  }
               }
            }
         }

         return result;
      }
      #endregion
      #endregion
   }

   #region Exceptions
   /// <summary>
   /// Exception thrown when a graph-specific error occurs.
   /// </summary>
   public class GraphException : ApplicationException
   {
      public GraphException()
         : base()
      {
      }

      public GraphException( String message )
         : base( message )
      {
      }

      protected GraphException( SerializationInfo info, StreamingContext context )
         : base( info, context )
      {
      }

      public GraphException( String message, Exception innerException )
         : base( message, innerException )
      {
      }
   }

   /// <summary>
   /// Exception thrown when a graph cycle is detected and a graph cycle would be invalid.
   /// </summary>
   public class GraphCycleDetectedException : GraphException
   {
      public GraphCycleDetectedException()
         : base()
      {
      }

      public GraphCycleDetectedException( String message )
         : base( message )
      {
      }

      protected GraphCycleDetectedException( SerializationInfo info, StreamingContext context )
         : base( info, context )
      {
      }

      public GraphCycleDetectedException( String message, Exception innerException )
         : base( message, innerException )
      {
      }
   }
   #endregion

   #region Internal Objects
   /// <summary>
   /// Stores a graph node including its predecessors and successors.
   /// </summary>
   /// <typeparam name="T">Graph node ID type.</typeparam>
   public class GraphNode<T> : IEquatable<GraphNode<T>>, IDisposable
   {
      /// <summary>
      /// Constructs a new graph node with the given unique node ID.
      /// </summary>
      /// <param name="id">unique node ID</param>
      public GraphNode( T id )
         : this( id, new List<GraphNode<T>>(), new List<GraphNode<T>>() )
      {
      }

      internal GraphNode( T id, List<GraphNode<T>> destinationForNodes, List<GraphNode<T>> sourceForNodes )
      {
         Id = id;
         Precedessors = destinationForNodes;
         Successors = sourceForNodes;
      }

      /// <summary>
      /// ID of the node.
      /// </summary>
      public T Id
      {
         get;
         protected set;
      }

      /// <summary>
      /// List of graph nodes coming into the current node.
      /// </summary>
      public List<GraphNode<T>> Precedessors
      {
         get;
         protected set;
      }

      /// <summary>
      /// List of graph nodes the current node goes to.
      /// </summary>
      public List<GraphNode<T>> Successors
      {
         get;
         protected set;
      }

      #region Object Overrides
      public override int GetHashCode()
      {
         int result = base.GetHashCode();

         result ^= Id.GetHashCode();
         // The Predecessors and Successors should not be used for the hash code as they are mutable. The ID is unique and immutable.

         return result;
      }

      public override bool Equals( Object obj )
      {
         GraphNode<T> node = obj as GraphNode<T>;

         bool result;

         if( result = node != null )
            result = Equals( node );

         return result;
      }
      #endregion

      #region Interface Implementations
      #region IDisposable Members
      public void Dispose()
      {
         Precedessors.Clear();
         Successors.Clear();
      }
      #endregion

      #region IEquatable<GraphNode<T>> Members
      public bool Equals( GraphNode<T> other )
      {
         bool result = Object.ReferenceEquals( this, other );

         if( !result )
            result = Id.Equals( other.Id );

         return result;
      }
      #endregion
      #endregion
   }

   /// <summary>
   /// Stores a graph connection consisting of a starting node, an ending node, and a connection weight.
   /// </summary>
   /// <typeparam name="T">Graph node ID type.</typeparam>
   public class GraphConnection<T> : IEquatable<GraphConnection<T>>, IDisposable
   {
      #region Monatomic Connection Counter
      private static UInt64 counter = 0;

      private static UInt64 GetNextId()
      {
         return counter++;
      }
      #endregion

      /// <summary>
      /// Constructs a new graph connection with the given source node, destination node, and connection weight.
      /// </summary>
      /// <param name="source">source node</param>
      /// <param name="destination">destination node</param>
      /// <param name="weight">connection weight</param>
      /// <param name="weightTolerance">connection weight tolerance</param>
      public GraphConnection( GraphNode<T> source, GraphNode<T> destination, Double weight, Double weightTolerance )
      {
         Id = GetNextId();
         Source = source;
         Destination = destination;
         Weight = weight;
         SetWeightTolerance( weightTolerance );
      }

      /// <summary>
      /// Internal connection ID.
      /// </summary>
      protected internal UInt64 Id
      {
         get;
         private set;
      }

      /// <summary>
      /// The graph connection's source.
      /// </summary>
      public GraphNode<T> Source
      {
         get;
         protected set;
      }

      /// <summary>
      /// The graph connection's destination.
      /// </summary>
      public GraphNode<T> Destination
      {
         get;
         protected set;
      }

      /// <summary>
      /// The graph connection's weight.
      /// </summary>
      public Double Weight
      {
         get;
         protected set;
      }

      /// <summary>
      /// The graph connection's weight tolerance.
      /// </summary>
      public Double WeightTolerance
      {
         get;
         private set;
      }

      /// <summary>
      /// Setter for weight tolerance. Does range validation.
      /// </summary>
      /// <param name="value">new weight tolerance</param>
      protected void SetWeightTolerance( Double value )
      {
         if( value >= 0.0 )
            WeightTolerance = value;
         else
            throw new ArgumentOutOfRangeException( "value", value, "Weight tolerance must be greater than or equal to zero." );
      }

      #region Object Overloads
      public override int GetHashCode()
      {
         int result = base.GetHashCode();

         result ^= Id.GetHashCode();
         result ^= Source.GetHashCode();
         result ^= Destination.GetHashCode();
         result ^= Weight.GetHashCode();

         return result;
      }

      public override bool Equals( Object obj )
      {
         GraphConnection<T> connection = obj as GraphConnection<T>;

         bool result;

         if( result = connection != null )
            result = Equals( connection );

         return result;
      }
      #endregion

      #region Interface Implementations
      #region IDisposable Members
      public void Dispose()
      {
      }
      #endregion

      #region IEquatable<GraphConnection<T>> Members
      public bool Equals( GraphConnection<T> other )
      {
         bool result = Object.ReferenceEquals( this, other );

         if( !result )
            result = Source.Equals( other.Source ) && Destination.Equals( other.Destination ) && Math.Abs( Weight - other.Weight ) <= WeightTolerance;

         return result;
      }
      #endregion
      #endregion
   }
   #endregion
}
