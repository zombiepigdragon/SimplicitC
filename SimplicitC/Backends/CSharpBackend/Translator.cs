using Antlr4.Runtime.Misc;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace SimplicitC.Backends.CSharpBackend
{
    class Translator : SimplicitCBaseVisitor<SyntaxNode>
    {
        public SyntaxTree ParseTree(SimplicitCParser.ProgramContext root)
        {
            var node = VisitProgram(root);
            return node.SyntaxTree;
        }

        public override SyntaxNode VisitAddexpression([NotNull] SimplicitCParser.AddexpressionContext context)
        {
            return BinaryExpression(
                SyntaxKind.AddExpression,
                (ExpressionSyntax)VisitExpression(context.expression(0)),
                (ExpressionSyntax)VisitExpression(context.expression(1))
                );
        }

        public override SyntaxNode VisitBlockstatement([NotNull] SimplicitCParser.BlockstatementContext context)
        {
            var statements = new SyntaxList<SyntaxNode>();
            foreach (var statement in context.statement())
            {
                statements = statements.Add(VisitStatement(statement));
            }
            return Block(statements);
        }

        public override SyntaxNode VisitDatatype([NotNull] SimplicitCParser.DatatypeContext context)
        {
            if (context.INTKEYWORD() != null)
            {
                return PredefinedType(
                    Token(SyntaxKind.IntKeyword)
                    );
            }
            else if (context.FLOATKEYWORD() != null)
            {
                return PredefinedType(
                    Token(SyntaxKind.FloatKeyword)
                    );
            }
            else if (context.BOOLKEYWORD() != null)
            {
                return PredefinedType(
                    Token(SyntaxKind.BoolKeyword)
                    );
            }
            else if(context.STRINGKEYWORD() != null)
            {
                return PredefinedType(
                    Token(SyntaxKind.StringKeyword)
                    );
            }
            return IdentifierName(context.GetText());
        }

        public override SyntaxNode VisitExpression([NotNull] SimplicitCParser.ExpressionContext context)
        {
            if (context.rawvalueexpression() != null)
            {
                return VisitRawvalueexpression(context.rawvalueexpression());
            }
            else if (context.functioncall() != null)
            {
                return VisitFunctioncall(context.functioncall());
            }
            else if (context.variable() != null)
            {
                return VisitVariable(context.variable());
            }
            else if (context.variableassignment() != null)
            {
                return VisitVariableassignment(context.variableassignment());
            }
            throw new NotImplementedException();
        }

        public override SyntaxNode VisitForloop([NotNull] SimplicitCParser.ForloopContext context)
        {
            return ForStatement(
                (StatementSyntax)VisitBlockstatement(context.blockstatement())
                )
                .WithInitializers(
                new SeparatedSyntaxList<SyntaxNode>().Add(VisitExpression(context.expression(0)))
                )
                .WithCondition(
                (ExpressionSyntax)VisitExpression(context.expression(1))
                )
                .WithIncrementors(
                new SeparatedSyntaxList<SyntaxNode>().Add(VisitExpression(context.expression(2)))
                );
        }

        public override SyntaxNode VisitFunctioncall([NotNull] SimplicitCParser.FunctioncallContext context)
        {
            var args = new SeparatedSyntaxList<ArgumentSyntax>();
            foreach (var arg in context.expression())
            {
                args = args.Add(
                    Argument(
                        (ExpressionSyntax)Visit(arg)
                        )
                    );
            }
            return InvocationExpression(
                IdentifierName(context.IDENTIFIER().GetText()))
                .WithArgumentList(
                ArgumentList(args)
                );
        }

        public override SyntaxNode VisitFunctiondeclaration([NotNull] SimplicitCParser.FunctiondeclarationContext context)
        {
            //TODO: Allow declaring methods
            //return MethodDeclaration(

            //    );
            throw new NotImplementedException();
        }

        public override SyntaxNode VisitLessthanexpression([NotNull] SimplicitCParser.LessthanexpressionContext context)
        {
            return BinaryExpression(
                SyntaxKind.LessThanExpression,
                (ExpressionSyntax)VisitExpression(context.expression(0)),
                (ExpressionSyntax)VisitExpression(context.expression(1))
                );
        }

        public override SyntaxNode VisitProgram([NotNull] SimplicitCParser.ProgramContext context)
        {
            var statements = new List<StatementSyntax>();
            foreach (var statement in context.statement())
            {
                statements.Add((StatementSyntax)VisitStatement(statement));
            }

            var node = CompilationUnit();
            node = node.WithUsings(
            SingletonList(
                UsingDirective(
                    QualifiedName(
                        IdentifierName("SimplicitCStdLib"),
                        IdentifierName("Globals")
                        )
                    )
                .WithStaticKeyword(Token(SyntaxKind.StaticKeyword))
                )
            )
            .WithMembers(
            SingletonList<MemberDeclarationSyntax>(
                NamespaceDeclaration(
                    QualifiedName(
                        IdentifierName("SimplicitC"),
                        IdentifierName("Runtime")
                        )
                    )
                .WithMembers(
                    SingletonList<MemberDeclarationSyntax>(
                        ClassDeclaration("SimCMainModule")
                        .WithMembers(
                            SingletonList<MemberDeclarationSyntax>(
                                MethodDeclaration(
                                    PredefinedType(
                                        Token(SyntaxKind.VoidKeyword)
                                        ),
                                    Identifier("_EntryPoint")
                                    )
                                .WithModifiers(
                                    TokenList(
                                        Token(SyntaxKind.PublicKeyword)
                                        )
                                    )
                                .WithBody(
                                    Block(
                                        statements
                                        )
                                    )
                                )
                            )
                        .WithBaseList(
                            BaseList(
                                new SeparatedSyntaxList<BaseTypeSyntax>().Add(
                                    SimpleBaseType(
                                            IdentifierName("IModule")
                                        )
                                    )
                                )
                            )
                        )
                    )
                )
            );
#if DEBUG
            node = node.NormalizeWhitespace();
#endif //DEBUG
            return node;
        }

        public override SyntaxNode VisitRawvalueexpression([NotNull] SimplicitCParser.RawvalueexpressionContext context)
        {
            if (context.INTEGER() != null)
            {
                return LiteralExpression(
                    SyntaxKind.NumericLiteralExpression,
                    Literal(int.Parse(context.INTEGER().GetText()))
                    );
            }
            else if (context.STRING() != null)
            {
                string text = context.STRING().GetText();
                text = text.Substring(1, text.Length - 2);
                return LiteralExpression(
                    SyntaxKind.StringLiteralExpression,
                    Literal(text)
                    );
            }
            throw new NotImplementedException();
        }

        public override SyntaxNode VisitStatement([NotNull] SimplicitCParser.StatementContext context)
        {
            if (context.forloop() != null)
            {
                return VisitForloop(context.forloop());
            }
            else if (context.whileloop() != null)
            {
                return VisitWhileloop(context.whileloop());
            }
            else if (context.variabledeclaration() != null)
            {
                return VisitVariabledeclaration(context.variabledeclaration());
            }
            else if (context.expression() != null)
            {
                return ExpressionStatement((ExpressionSyntax)Visit(context.expression()));
            }
            throw new NotImplementedException();
        }

        public override SyntaxNode VisitVariable([NotNull] SimplicitCParser.VariableContext context)
        {
            return IdentifierName(context.IDENTIFIER().GetText());
        }

        public override SyntaxNode VisitVariableassignment([NotNull] SimplicitCParser.VariableassignmentContext context)
        {
            return AssignmentExpression(
                SyntaxKind.SimpleAssignmentExpression,
                (ExpressionSyntax)VisitVariable(context.variable()),
                (ExpressionSyntax)VisitExpression(context.expression())
                );
        }

        public override SyntaxNode VisitVariabledeclaration([NotNull] SimplicitCParser.VariabledeclarationContext context)
        {
            return LocalDeclarationStatement(
                VariableDeclaration(
                    (TypeSyntax)VisitDatatype(context.datatype())
                    )
                .WithVariables(
                    SingletonSeparatedList(
                        VariableDeclarator(
                            Identifier(context.variable().GetText())
                            )
                        .WithInitializer(
                            EqualsValueClause(
                                (ExpressionSyntax)VisitExpression(context.expression())
                                )
                            )
                        )
                    )
                );
        }

        public override SyntaxNode VisitWhileloop([NotNull] SimplicitCParser.WhileloopContext context)
        {
            return WhileStatement(
                (ExpressionSyntax)VisitExpression(context.expression()),
                (StatementSyntax)VisitBlockstatement(context.blockstatement())
                );
        }
    }
}
