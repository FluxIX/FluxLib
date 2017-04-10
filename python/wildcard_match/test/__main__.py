import unittest
from ..wildcard_match import WildcardMatch

class WildcardTestCase( unittest.TestCase ):
   def _prepare_parameters( self, *params ):
      return [ param.split( "." ) for param in params ]

   def _test_match__positive( self, target, pattern ):
      self.assertTrue( WildcardMatch.match( *self._prepare_parameters( target, pattern ) ) )

   def _test_match__negative( self, target, pattern ):
      self.assertFalse( WildcardMatch.match( *self._prepare_parameters( target, pattern ) ) )

class WildcardTests_ReplaceOne( WildcardTestCase ):
   def test_two_sequential__positive( self ):
      self._test_match__positive( "Character.Action", "?.?" )

   def test_three_sequential__negative( self ):
      self._test_match__negative( "Character.Action", "?.?.?" )

   def test_with_following_invariant__positive( self ):
      self._test_match__positive( "Character.Action", "?.Action" )

   def test_with_following_invariant__negative( self ):
      self._test_match__negative( "Character.Scope.Action", "?.Action" )

   def test_single_item__positive( self ):
      self._test_match__positive( "Character", "?" )

   def test_single_item__negative( self ):
      self._test_match__negative( "Character.Action", "?" )

class WildcardTests_ReplaceZeroOrMore( WildcardTestCase ):
   def test_entire_expression__positive( self ):
      self._test_match__positive( "Character.Action", "*" )

   def test_with_following_one_invariant__positive( self ):
      self._test_match__positive( "Character.Action", "*.Action" )

   def test_with_following_two_invariants__positive( self ):
      self._test_match__positive( "Character.Scope.Action", "*.Action" )

class WildcardTests_ReplaceOneOrMore( WildcardTestCase ):
   def test_one_item__positive( self ):
      self._test_match__positive( "Character", "+" )

   def test_two_items__positive( self ):
      self._test_match__positive( "Character.Action", "+" )

class WildcardTests_ReplaceRange( WildcardTestCase ):
   def test_two_items__positive( self ):
      self._test_match__positive( "Character.Action", "{2,2}" )

   def test_zero_to_two_items__negative( self ):
      self._test_match__negative( "Character.Scope.Action", "{0,2}" )

   def test_at_least_two_items__positive( self ):
      self._test_match__positive( "Character.Action", "{2,}" )

   def test_at_least_three_items__negative( self ):
      self._test_match__negative( "Character.Action", "{3,}" )

if __name__ == "__main__":
   unittest.main()
