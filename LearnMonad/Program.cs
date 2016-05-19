using System;

namespace LearnMonad
{
	class Maybe<T>
	{
		public readonly static Maybe<T> Nothing = new Maybe<T>();
		public T Value { get; private set; }
		public bool HasValue { get; private set; }
		Maybe()
		{
			HasValue = false;
		}
		public Maybe(T value)
		{
			Value = value;
			HasValue = true;
		}
	}

	class Function<T, U>{
		public U value  { get; private set; }
		public Func<T, U> map;
		public Function(Func<T, U> f) { this.map = f; }
		public U invoke(T arg){this.value = this.map(arg); return this.value;}
	}

	class Identity<T> : Function<T, T>
	{
		public T Value { get; private set; }
		public Identity() : base(x => x) {  }
	}

	static class MainClass
	{
		public static Func<T1, Func<T2, TResult>> 
		Curry<T1, T2, TResult>(this Func<T1, T2, TResult> fn)
		{
			Func<Func<T1, T2, TResult>, Func<T1, Func<T2, TResult>>> curry = 
				f => x => y => f(x, y);
			return curry(fn);
		}


		public static Identity<T> Unit<T>(T value)
		{
			Identity<T> identity = new Identity<T>();
			identity.invoke (value);
			return identity;
		}

		public static Maybe<T> UnitMaybe<T>(T value)
		{
			return new Maybe<T> (value);
		}

		public static Identity<U> Bind<T,U>(Identity<T> id, Func<T,Identity<U>> k)
		{
			return k(id.Value);
		}

		public static Maybe<U> BindMaybe<T,U>(Maybe<T> id, Func<T,Maybe<U>> k)
		{
			if (id.HasValue)
				return k (id.Value);
			else
				return Maybe<U>.Nothing;
		}

		public static Func<T, V> Compose<T, U, V>(this Func<U, V> f, Func<T, U> g)
		{
			return x => f(g(x));
		}

		public static Function<T, V> Compose<T, U, V>(this Function<U, V> f, Function<T, U> g)
		{
			return new Function<T, V> (f.map.Compose (g.map));
		}

		public static void Main (string[] args)
		{
			Console.WriteLine ("Hello World!");
			var r = Bind(Unit(5), x => Bind(Unit(6), y => Unit(x + y)));
			Console.WriteLine(r.Value);

			Func<double, double> square = x => x * x;
			Func<double, double, double> add = (x, y) => x+y; 
			Func<double, double> add3 = add.Curry () (3);
			Func<double, Maybe<double>> UnitMaybeDouble = UnitMaybe<double>;
			Func<Maybe<double>, Func<double, Maybe<double>>, Maybe<double>> BindMaybeDouble = BindMaybe<double, double>;
			Func<Maybe<double>, Maybe<double>, Maybe<double>> addMaybe = (x, y) => BindMaybeDouble(x, 
				x1 => BindMaybeDouble(y, y1 => UnitMaybeDouble(add.Curry()(x1)(y1))) 
			);

			Function<Maybe<double>, Maybe<double>> squareMaybe = new Function<Maybe<double>, Maybe<double>> 
			(x => 
				BindMaybe(x, 
					UnitMaybeDouble.Compose(square) 
				)
			);

			Console.WriteLine (square (5));
			Console.WriteLine (square.Compose(add3) (5));
			Console.WriteLine (add3.Compose(square) (5));
			Maybe<double> square5 = squareMaybe.invoke (new Maybe<double> (5));
			Console.WriteLine ("HasValue="+square5.HasValue+"  square5="+square5.Value);
			square5 = squareMaybe.invoke (Maybe<double>.Nothing);
			Console.WriteLine ("HasValue="+square5.HasValue+"  square5="+square5.Value);

			var maybe5 = new Maybe<double> (5);
			var maybe3 = new Maybe<double> (3);
			var maybeNothing = Maybe<double>.Nothing;

			square5 = addMaybe(maybe5, maybe3);
			Console.WriteLine ("HasValue="+square5.HasValue+"  square5="+square5.Value);

			square5 = addMaybe(maybe5, maybeNothing);
			Console.WriteLine ("HasValue="+square5.HasValue+"  square5="+square5.Value);

			square5 = addMaybe(maybeNothing, maybeNothing);
			Console.WriteLine ("HasValue="+square5.HasValue+"  square5="+square5.Value);

			square5 = addMaybe(maybeNothing, maybe5);
			Console.WriteLine ("HasValue="+square5.HasValue+"  square5="+square5.Value);
		}
	}
}
