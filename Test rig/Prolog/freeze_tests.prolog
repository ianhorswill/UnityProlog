test(freeze(wakes_on_binding),
     [ true(Y == 1) ]) :-
   freeze(X, Y = 1),
   X = 1.

test(freeze(runs_goal_when_variable_already_bound),
     [ true(Y == 1) ]) :-
   X = 1,
   freeze(X, Y = 1).

test(freeze(frozen_retrieves_goal),
     [ true(Z == 1) ]) :-
   freeze(X, Y = 1),
   frozen(X, Y = Z).

test(freeze(frozen_fails_when_goal_pattern_doesnt_match)) :-
   freeze(X, _Y = 1),
   \+ frozen(X, a).

test(freeze(doesnt_wake_without_binding),
     [ true(var(Y)) ]) :-
   freeze(_X, Y = 1).

thaw(X) :-
   frozen(X, Goal) -> Goal ; true.

test(freeze(thawing),
     [ true(Y == 1) ]) :-
   freeze(X, Y = 1),
   thaw(X).

test(freeze(composition_of_frozen_goals),
     [ true(X == 1),
       true(Y == 2) ]) :-
   freeze(A, X = 1),
   freeze(B, Y = 2),
   A = B,
   A = 1.

test(freeze(dif0)) :-
   dif().

test(freeze(dif1)) :-
   dif(_).

test(freeze(dif2_completion)) :-
   dif(_, _).

test(freeze(dif_vars_are_different)) :-
   dif(X,_Y),
   X = 1.

test(freeze(dif_vars_bound_to_each_other)) :-
   \+ (dif(A,B), A=B),
   \+ (dif(B,A), A=B).

test(freeze(dif_vars_bound_independently_to_same_value)) :-
   \+ (dif(X,Y),
       X = 1,
       Y = 1).

test(freeze(dif_vars_bound_to_different_values)) :-
   dif(X,Y),
   X = 1,
   Y = 2.

test(freeze(dif_vars_bound_to_same_third_var)) :-
   \+ (dif(X,Y),
       X = Z,
       Y = Z).

test(freeze(dif_multiple_constraints)) :-
   dif(X,_), dif(X,_), X=1.
test(freeze(dif_multiple_constraints2)) :-
   \+ (dif(X,Y), dif(X,_), X=Y).
test(freeze(dif_multiple_constraints3)) :-
   \+ (dif(X,Y), dif(X,_), X=1, Y=1).
test(freeze(dif_multiple_constraints4)) :-
   \+ (dif(X,_), dif(X,Y), X=Y).
test(freeze(dif_multiple_constraints5)) :-
   \+ (dif(X,_), dif(X,Y), X=1, Y=1).

test(freeze(meta_meta_unify)) :-
   dif(A, 1), dif(B, 2), A=B.

test(freeze(dif_transitive_equation),
     [ true((var(A), var(B), var(C)))]) :-
   dif(A,B), dif(B,C), A=C.

test(freeze(dif3_completion)) :-
   dif(_, _, _).

test(freeze(dif3_succeed)) :-
   dif(A, B, C), A=1, B=2, C=3.

test(freeze(dif3_fail)) :-
   \+ (dif(A, B, C), A=1, B=2, C=1).

test(freeze(dif3:solutions),
     true(L == [1:2:3, 1:3:2, 2:1:3, 2:3:1, 3:1:2, 3:2:1])) :-
   all(A:B:C,
       ( dif(A, B, C),
	 member(A, [1,2,3]),
	 member(B, [1, 2, 3]),
	 member(C, [1, 2, 3]) ),
       L).
