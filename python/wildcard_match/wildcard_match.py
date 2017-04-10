import re

class WildcardMatch( object ):
   @staticmethod
   def _get_variant_lengths( match_object ):
      minimum_variant_length = match_object.groupdict()[ "min" ]
      if minimum_variant_length:
         minimum_variant_length = int( minimum_variant_length )
      else:
         minimum_variant_length = 0

      maximum_variant_length = match_object.groupdict()[ "max" ]
      if maximum_variant_length:
         maximum_variant_length = int( maximum_variant_length )
      else:
         maximum_variant_length = None

      if maximum_variant_length is not None and maximum_variant_length < minimum_variant_length:
         raise ValueError( "Maximum length ({:d}) is less than minimum length ({:d})." )
      else:
         return minimum_variant_length, maximum_variant_length

   @staticmethod
   def _sum_variant_lengths( *args ):
      result = None

      if args:
         minimum, maximum = args[ 0 ]
         index = 1

         while index < len( args ):
            next_min, next_max = args[ index ]
            index += 1

            minimum += next_min
            if next_max is not None:
               if maximum is None:
                  maximum = next_max
               elif maximum is not None:
                  maximum += next_max

         result = ( minimum, maximum )

      return result

   @staticmethod
   def match( target, pattern, **kwargs ):
      """
      Returns True if the target matches the pattern, False otherwise.

      Pattern Matching:
      *: matches zero or more occurrances
      ?: matches zero or one occurrance
      +: matches one or more occurrances
      {m,n}: matches m to n occurrances
      """

      variant_rewrite_rules = { "*": (0, None), "?": (0,1), "+": (1,None) }
      variant_pattern = r"\s*\{\s*(?P<min>\d*)\s*,\s*(?P<max>\d*)\s*\}\s*"
      variant_expression = re.compile( variant_pattern )

      pattern_length = len( pattern )
      target_length = len( target )

      target_index = 0
      piece_index = 0
      match_possible = True

      while match_possible and piece_index < pattern_length and target_index < target_length:
         piece = pattern[ piece_index ]

         mo = variant_expression.match( piece )
         if piece in variant_rewrite_rules or mo is not None:
            if mo is None:
               minimum_variant_length, maximum_variant_length = variant_rewrite_rules[ piece ]
            else:
               minimum_variant_length, maximum_variant_length = WildcardMatch._get_variant_lengths( mo )

            piece_index += 1
            if piece_index < pattern_length:
               # We collapse all of the variant symbols into a single variant sequence.

               next_piece = pattern[ piece_index ]
               mo = variant_expression.match( next_piece )
               while piece_index < pattern_length and ( next_piece in variant_rewrite_rules or mo is not None ):
                  if mo is None:
                     new_minimum_variant_length, new_maximum_variant_length = variant_rewrite_rules[ piece ]
                  else:
                     new_minimum_variant_length, new_maximum_variant_length = WildcardMatch._get_variant_lengths( mo )

                  minimum_variant_length, maximum_variant_length = WildcardMatch._sum_variant_lengths( ( minimum_variant_length, maximum_variant_length ), ( new_minimum_variant_length, new_maximum_variant_length ) )

                  piece_index += 1

                  if piece_index < pattern_length:
                     next_piece = pattern[ piece_index ]
                     mo = variant_expression.match( next_piece )

            if piece_index == pattern_length: # is this the last piece?
               remaining_target_amount = len( target[ target_index : ] )
               if remaining_target_amount >= minimum_variant_length and ( maximum_variant_length is None or maximum_variant_length <= remaining_target_amount ): # is the remaining target enough to satisfy the required variance?
                  if maximum_variant_length is None:
                     target_index = target_length
                  else:
                     target_index += maximum_variant_length
               else: # the required variance exceeds the remaining target length
                  match_possible = False
            else: # it is an invariant
               # A greedy match cannot be done here. A greedy match would consume the rest of the target.

               target_index += minimum_variant_length

               if maximum_variant_length is None:
                  next_piece_matched = False
                  while not next_piece_matched and target_index < target_length:
                     next_piece_matched = target[ target_index ] == next_piece
                     target_index += 1

                  match_possible = next_piece_matched
                  if next_piece_matched: # consume the pattern piece
                     piece_index += 1
               else:
                  target_index += maximum_variant_length - minimum_variant_length
         elif target[ target_index ] == piece:
            target_index += 1
            piece_index += 1
         else: # No possible match.
            match_possible = False

      result = target_index >= target_length and piece_index >= pattern_length

      return result
