﻿// Copyright (c) 2010-2013 AlphaSierraPapa for the SharpDevelop Team
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this
// software and associated documentation files (the "Software"), to deal in the Software
// without restriction, including without limitation the rights to use, copy, modify, merge,
// publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons
// to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
// PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE
// FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

using System;
using ICSharpCode.NRefactory.PatternMatching;
using ICSharpCode.NRefactory.TypeSystem;

namespace ICSharpCode.NRefactory.CSharp.Refactoring
{
	/// <summary>
	/// Helper class for constructing pattern ASTs.
	/// </summary>
	public class PatternHelper
	{
		/// <summary>
		/// Produces a choice pattern for <c>expr1 op expr2</c> or <c>expr2 op expr1</c>.
		/// </summary>
		public static Expression CommutativeOperator(Expression expr1, BinaryOperatorType op, Expression expr2)
		{
			return new Choice {
				new BinaryOperatorExpression(expr1, op, expr2),
				new BinaryOperatorExpression(expr2.Clone(), op, expr1.Clone())
			};
		}
		
		/// <summary>
		/// Optionally allows parentheses around the given expression.
		/// </summary>
		public static Expression OptionalParentheses(Expression expr)
		{
			return new OptionalParenthesesPattern(expr);
		}
		
		sealed class OptionalParenthesesPattern : Pattern
		{
			readonly INode child;
			
			public OptionalParenthesesPattern(INode child)
			{
				this.child = child;
			}
			
			public override bool DoMatch(INode other, Match match)
			{
				INode unpacked = ParenthesizedExpression.UnpackParenthesizedExpression(other as Expression);
				return child.DoMatch(unpacked, match);
			}
		}
	
		/// <summary>
		/// Allows to give parameter declaration group names.
		/// </summary>
		public static ParameterDeclaration NamedParameter(string groupName)
		{
			return new NamedParameterDeclaration (groupName);
		}

		/// <summary>
		/// Allows to give parameter declaration group names.
		/// </summary>
		public static ParameterDeclaration NamedParameter(string groupName, AstType type, string name, ParameterModifier modifier = ParameterModifier.None)
		{
			return new NamedParameterDeclaration (groupName, type, name, modifier);
		}

		sealed class NamedParameterDeclaration : ParameterDeclaration
		{
			readonly string groupName;
			public string GroupName {
				get { return groupName; }
			}

			public NamedParameterDeclaration(string groupName = null)
			{
				this.groupName = groupName;
			}

			public NamedParameterDeclaration(string groupName, AstType type, string name, ParameterModifier modifier = ParameterModifier.None) : base (type, name, modifier)
			{
				this.groupName = groupName;
			}

			protected internal override bool DoMatch(AstNode other, Match match)
			{
				match.Add(this.groupName, other);
				return base.DoMatch(other, match);
			}
		}

		/// <summary>
		/// Matches any type
		/// </summary>
		public static AstType AnyType (bool doesMatchNullTypes = false)
		{
			return new InternalAnyType(doesMatchNullTypes);
		}

		/// <summary>
		/// Matches any type
		/// </summary>
		public static AstType AnyType (string groupName, bool doesMatchNullTypes = false)
		{
			return new InternalAnyType(doesMatchNullTypes, groupName);
		}

		/// <summary>
		/// Matches any other type
		/// </summary>
		sealed class InternalAnyType : AstType
		{
			readonly string groupName;
			readonly bool doesMatchNullTypes;

			public string GroupName {
				get { return groupName; }
			}

			public InternalAnyType(bool doesMatchNullTypes, string groupName = null)
			{
				this.doesMatchNullTypes = doesMatchNullTypes;
				this.groupName = groupName;
			}


			public override ITypeReference ToTypeReference(NameLookupMode lookupMode, InterningProvider interningProvider)
			{
				throw new InvalidOperationException();
			}

			public override void AcceptVisitor (IAstVisitor visitor)
			{
				throw new InvalidOperationException();
			}

			public override T AcceptVisitor<T> (IAstVisitor<T> visitor)
			{
				throw new InvalidOperationException();
			}

			public override S AcceptVisitor<T, S> (IAstVisitor<T, S> visitor, T data)
			{
				throw new InvalidOperationException();
			}

			protected internal override bool DoMatch(AstNode other, ICSharpCode.NRefactory.PatternMatching.Match match)
			{
				match.Add(this.groupName, other);
				return other is AstType && (doesMatchNullTypes || !other.IsNull);
			}
		}
	}
}
