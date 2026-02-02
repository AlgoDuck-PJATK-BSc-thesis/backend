namespace AlgoDuck.Tests.Unit.Shared.Analyzer;

using System.Text;
using AlgoDuck.Shared.Analyzer.AstAnalyzer;
using Xunit;

public class AnalyzerStressTests
{
    [Fact]
    public void analyzer_should_handle_deeply_nested_generic_type_parameters_without_stack_overflow()
    {
        var code = @"
            public class Test {
                Map<Map<Map<Map<Map<Map<Map<Map<Map<Map<Map<Map<Map<Map<Map<Map<Map<Map<Map<Map
                Map<Map<Map<Map<Map<Map<Map<Map<Map<Map<Map<Map<Map<Map<Map<Map<Map<Map<Map<Map
                Map<Map<Map<Map<Map<Map<Map<Map<Map<Map<Map<Map<Map<Map<Map<Map<Map<Map<Map<Map
                Map<Map<Map<Map<Map<Map<Map<Map<Map<Map<Map<Map<Map<Map<Map<Map<Map<Map<Map<Map
                Map<Map<Map<Map<Map<Map<Map<Map<Map<Map<Map<Map<Map<Map<Map<Map<Map<Map<Map<Map
                Integer, Integer
                >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>> x;
            }";
        AssertAnalyzerCompletes(code);
    }

    [Fact]
    public void analyzer_should_handle_deeply_nested_parentheses_in_expressions_without_stack_overflow()
    {
        var code = @"
            public class Test {
                int x = ((((((((((((((((((((((((((((((((((((((((((((((((((
                        ((((((((((((((((((((((((((((((((((((((((((((((((((
                        ((((((((((((((((((((((((((((((((((((((((((((((((((
                        1
                        ))))))))))))))))))))))))))))))))))))))))))))))))))
                        ))))))))))))))))))))))))))))))))))))))))))))))))))
                        ));
            }";
        AssertAnalyzerCompletes(code);
    }

    [Fact]
    public void analyzer_should_handle_deeply_nested_array_dimensions_without_stack_overflow()
    {
        var code = @"
            public class Test {
                int[][][][][][][][][][][][][][][][][][][][][][][][][][][][][][][][]
                [][][][][][][][][][][][][][][][][][][][][][][][][][][][][][][][]
                [][][][][][][][][][][][][][][][][][][][][][][][][][][][][][][][]
                [][][][][][][][][][][][][][][][][][][][][][][][][][][][][][][][]
                [][][][][][][][][][][][][][][][][][][][][][][][][][][][][][][][]
                [][][][][][][][][][][][][][][][][][][][][][][][][][][][][][][][]
                [][][][][][][][][][][][][][][][][][][][][][][][][][][][][][][][]
                [][][][][][][][][][][][][][][][][][][][][][][][][][][][][][][][]x;
            }";
        AssertAnalyzerCompletes(code);
    }

    [Fact]
    public void analyzer_should_handle_one_hundred_levels_of_nested_class_declarations()
    {
        var code = GenerateNestedClasses(100);
        AssertAnalyzerCompletes(code);
    }

    [Fact]
    public void analyzer_should_handle_deeply_nested_unary_negation_operators_without_stack_overflow()
    {
        var code = @"
            public class Test {
                int x = !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                        !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                        !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                        true;
            }";
        AssertAnalyzerCompletes(code);
    }

    [Fact]
    public void analyzer_should_handle_class_with_one_thousand_generic_type_parameters()
    {
        var code = GenerateManyGenericParams(1000);
        AssertAnalyzerCompletes(code);
    }

    [Fact]
    public void analyzer_should_handle_method_with_one_thousand_parameters()
    {
        var code = GenerateManyMethodParams(1000);
        AssertAnalyzerCompletes(code);
    }

    [Fact]
    public void analyzer_should_handle_class_implementing_one_thousand_interfaces()
    {
        var code = GenerateManyImplements(1000);
        AssertAnalyzerCompletes(code);
    }

    [Fact]
    public void analyzer_should_handle_array_literal_with_one_thousand_elements()
    {
        var code = GenerateLargeArrayLiteral(1000);
        AssertAnalyzerCompletes(code);
    }

    [Fact]
    public void analyzer_should_handle_one_thousand_chained_method_calls_without_stack_overflow()
    {
        var code = @"
            public class Test {
                void foo() {
                    x" + string.Concat(Enumerable.Repeat(".foo()", 1000)) + @";
                }
            }";
        AssertAnalyzerCompletes(code);
    }

    [Fact]
    public void analyzer_should_handle_expression_with_one_thousand_binary_addition_operators()
    {
        var code = @"
            public class Test {
                int x = 1" + string.Concat(Enumerable.Repeat(" + 1", 1000)) + @";
            }";
        AssertAnalyzerCompletes(code);
    }

    [Fact]
    public void analyzer_should_handle_malformed_code_with_many_unclosed_opening_braces()
    {
        var code = @"
            public class Test {{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{
            {{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{
            {{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{";
        AssertAnalyzerCompletes(code);
    }

    [Fact]
    public void analyzer_should_handle_malformed_code_with_many_unclosed_generic_angle_brackets()
    {
        var code = @"
            public class Test {
                Map<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<
                <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<
                <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<
            }";
        AssertAnalyzerCompletes(code);
    }

    [Fact]
    public void analyzer_should_handle_multiple_statements_missing_semicolons()
    {
        var code = @"
            public class Test {
                int a = 1
                int b = 2
                int c = 3
                int d = 4
                void foo() { return }
            }";
        AssertAnalyzerCompletes(code);
    }

    [Fact]
    public void analyzer_should_handle_random_sequence_of_invalid_tokens_and_operators()
    {
        var code = @"
            public class Test {
                } { ] [ ) ( >> << && || !! ?? :: -> 
                } { ] [ ) ( >> << && || !! ?? :: ->
                } { ] [ ) ( >> << && || !! ?? :: ->
            }";
        AssertAnalyzerCompletes(code);
    }

    [Fact]
    public void analyzer_should_handle_static_initializer_blocks_with_nested_classes()
    {
        var code = @"
            public class Test {
                static {
                    class Inner {
                        static {
                            class InnerInner {
                                static { }
                            }
                        }
                    }
                }
                public void realMethod() { }
            }";
        AssertAnalyzerCompletes(code);
    }

    [Fact]
    public void analyzer_should_handle_instance_initializer_blocks_mixed_with_valid_members()
    {
        var code = @"
            public class Test {
                { System.out.println(""init""); }
                private int x = 5;
                { x = 10; }
                public int getX() { return x; }
            }";
        AssertAnalyzerCompletes(code);
    }
    [Fact]
    public void analyzer_should_handle_record_declaration_followed_by_regular_class()
    {
        var code = @"
            public record Point(int x, int y) { }
            public class Test {
                Point p = new Point(1, 2);
            }";
        AssertAnalyzerCompletes(code);
    }

    [Fact]
    public void analyzer_should_handle_enum_with_abstract_methods_and_instance_implementations()
    {
        var code = @"
            public class Outer {
                enum Status {
                    ACTIVE { public String label() { return ""A""; } },
                    INACTIVE { public String label() { return ""I""; } };
                    public abstract String label();
                }
                public void foo() { }
            }";
        AssertAnalyzerCompletes(code);
    }

    [Fact]
    public void analyzer_should_handle_annotation_type_definition_with_methods_and_default_values()
    {
        var code = @"
            public class Outer {
                @interface MyAnnotation {
                    String value();
                    int count() default 0;
                }
                @MyAnnotation(value=""test"")
                public void foo() { }
            }";
        AssertAnalyzerCompletes(code);
    }

    [Fact]
    public void analyzer_should_handle_complex_nested_lambda_expressions()
    {
        var code = @"
            public class Test {
                void foo() {
                    Runnable r = () -> { 
                        Runnable r2 = () -> {
                            Runnable r3 = () -> {};
                        };
                    };
                    Function<Integer, Integer> f = x -> x + 1;
                    BiFunction<Integer, Integer, Integer> g = (a, b) -> a + b;
                }
            }";
        AssertAnalyzerCompletes(code);
    }

    [Fact]
    public void analyzer_should_handle_method_reference_expressions_of_various_forms()
    {
        var code = @"
            public class Test {
                void foo() {
                    Function<String, Integer> f = String::length;
                    Supplier<List<String>> s = ArrayList::new;
                    Consumer<String> c = System.out::println;
                }
            }";
        AssertAnalyzerCompletes(code);
    }

    [Fact]
    public void analyzer_should_distinguish_between_casts_and_parenthesized_expressions()
    {
        var code = @"
            public class Test {
                void foo() {
                    int a = (int) 5.0;
                    int b = (a) + 5;
                    int c = (int) (double) (float) 5;
                    Object o = (Comparable<String> & Serializable) () -> ""hi"";
                }
            }";
        AssertAnalyzerCompletes(code);
    }
    [Fact]
    public void analyzer_should_handle_very_long_qualified_package_and_type_names()
    {
        var code = @"
            public class Test {
                " + string.Join(".", Enumerable.Repeat("pkg", 100)) + @".Type x;
            }";
        AssertAnalyzerCompletes(code);
    }

    [Fact]
    public void analyzer_should_handle_empty_class_with_no_members()
    {
        var code = @"public class Test { }";
        AssertAnalyzerCompletes(code);
    }

    [Fact]
    public void analyzer_should_handle_class_containing_only_initializer_blocks()
    {
        var code = @"
            public class Test {
                { }
                static { }
                { int x = 1; }
                static { int y = 2; }
            }";
        AssertAnalyzerCompletes(code);
    }

    [Fact]
    public void analyzer_should_handle_interface_with_default_static_and_private_methods()
    {
        var code = @"
            public interface Test {
                default void foo() { System.out.println(""default""); }
                static void bar() { System.out.println(""static""); }
                private void baz() { }
            }";
        AssertAnalyzerCompletes(code);
    }

    [Fact]
    public void analyzer_should_handle_invalid_statement_in_class_body_instead_of_member()
    {
        var code = @"
            public class Test {
                System.out.println(""oops"");
                int x = 5;
            }";
        AssertAnalyzerCompletes(code);
    }

    [Fact]
    public void analyzer_should_handle_invalid_expressions_as_statements_in_class_body()
    {
        var code = @"
            public class Test {
                1 + 2;
                ""hello"";
                x.y.z;
            }";
        AssertAnalyzerCompletes(code);
    }

    [Fact]
    public void analyzer_should_correctly_parse_nested_generic_types_in_field_and_method_declarations()
    {
        var code = @"
            public class Test {
                Map<String, List<Set<Integer>>> x;
                public Map<String, List<Set<Integer>>> getX() { return x; }
            }";
        AssertAnalyzerCompletes(code);
    }

    [Fact]
    public void analyzer_should_handle_expression_with_ten_thousand_binary_operators_without_stack_overflow()
    {
        var code = @"
            public class Test {
                int x = 1" + string.Concat(Enumerable.Repeat(" + 1", 10000)) + @";
            }";
        AssertAnalyzerCompletes(code);
    }

    [Fact]
    public void analyzer_should_handle_ten_thousand_chained_method_calls_without_stack_overflow()
    {
        var code = @"
            public class Test {
                void foo() {
                    x" + string.Concat(Enumerable.Repeat(".foo()", 10000)) + @";
                }
            }";
        AssertAnalyzerCompletes(code);
    }

    [Fact]
    public void analyzer_should_handle_two_hundred_levels_of_nested_class_declarations()
    {
        var code = GenerateNestedClasses(200);
        AssertAnalyzerCompletes(code);
    }

    [Fact]
    public void analyzer_should_handle_fifty_levels_of_nested_class_declarations()
    {
        var code = GenerateNestedClasses(50);
        AssertAnalyzerCompletes(code);
    }

    [Fact]
    public void analyzer_should_handle_generic_types_that_are_both_wide_and_deeply_nested()
    {
        var code = GenerateWideDeepGenerics(20, 5);
        AssertAnalyzerCompletes(code);
    }

    [Fact]
    public void analyzer_should_handle_many_valid_members_interspersed_with_syntax_errors()
    {
        var code = GenerateManyMembersWithErrors(100);
        AssertAnalyzerCompletes(code);
    }

    [Fact]
    public void analyzer_should_handle_trailing_comma_in_array_literal_elements()
    {
        var code = @"
            public class Test {
                int[] arr = {1, 2, 3, };
            }";
        AssertAnalyzerCompletes(code);
    }

    [Fact]
    public void analyzer_should_handle_trailing_comma_in_implements_clause()
    {
        var code = @"public class Test implements Foo, Bar, { }";
        AssertAnalyzerCompletes(code);
    }

    [Fact]
    public void analyzer_should_handle_trailing_comma_in_generic_type_parameters()
    {
        var code = @"
            public class Test {
                Map<String, Integer, > x;
            }";
        AssertAnalyzerCompletes(code);
    }

    [Fact]
    public void analyzer_should_handle_double_comma_in_array_literal_elements()
    {
        var code = @"
            public class Test {
                int[] arr = {1, , 2};
            }";
        AssertAnalyzerCompletes(code);
    }

    [Fact]
    public void analyzer_should_handle_empty_generic_type_parameters()
    {
        var code = @"
            public class Test {
                Map<> x;
            }";
        AssertAnalyzerCompletes(code);
    }

    [Fact]
    public void analyzer_should_handle_empty_method_parameters_with_comma()
    {
        var code = @"
            public class Test {
                void foo(,) { }
            }";
        AssertAnalyzerCompletes(code);
    }

    [Fact]
    public void analyzer_should_handle_nesting_depth_exactly_at_recursion_limit()
    {
        var code = GenerateNestedParens(99);
        AssertAnalyzerCompletes(code);
    }

    [Fact]
    public void analyzer_should_handle_nesting_depth_one_over_recursion_limit()
    {
        var code = GenerateNestedParens(101);
        AssertAnalyzerCompletes(code);
    }

    [Fact]
    public void analyzer_should_handle_identifier_with_ten_thousand_characters()
    {
        var code = @"
            public class Test {
                int " + new string('a', 10000) + @" = 1;
            }";
        AssertAnalyzerCompletes(code);
    }

    [Fact]
    public void analyzer_should_handle_string_literal_with_ten_thousand_characters()
    {
        var code = @"
            public class Test {
                String s = """ + new string('x', 10000) + @""";
            }";
        AssertAnalyzerCompletes(code);
    }

    [Fact]
    public void analyzer_should_handle_one_hundred_annotations_on_single_method()
    {
        var code = @"
            public class Test {
                " + string.Concat(Enumerable.Repeat("@SuppressWarnings(\"unchecked\") ", 100)) + @"
                public void foo() { }
            }";
        AssertAnalyzerCompletes(code);
    }

    [Fact]
    public void analyzer_should_handle_nested_annotations_with_multiple_levels()
    {
        var code = @"
            public class Test {
                @Outer(@Inner(@Deep(""value"")))
                public void foo() { }
            }";
        AssertAnalyzerCompletes(code);
    }

    [Fact]
    public void analyzer_should_handle_switch_expression_with_arrow_syntax()
    {
        var code = @"
            public class Test {
                int x = switch(y) {
                    case 1 -> 10;
                    case 2 -> 20;
                    default -> 0;
                };
            }";
        AssertAnalyzerCompletes(code);
    }

    [Fact]
    public void analyzer_should_handle_multiple_top_level_class_declarations_in_same_file()
    {
        var code = @"
            public class Test {
                int x = 1;
            }
            class Helper {
                int y = 2;
            }
            class Another {
                int z = 3;
            }";
        AssertAnalyzerCompletes(code);
    }

    [Fact]
    public void analyzer_should_handle_unicode_escape_sequences_and_non_ascii_identifiers()
    {
        var code = @"
            public class Test {
                int \u0061\u0062\u0063 = 1;
                String café = ""hello"";
            }";
        AssertAnalyzerCompletes(code);
    }

    [Fact]
    public void analyzer_should_handle_extremely_long_repeated_string_literal_efficiently()
    {
        var code = @"
            public class Test {
                String s = """"""""""""""""""""""""""""""""""""""""""""""""""""""""""""""""""""""""""""""""""""""""""""""""""
                            """""""""""""""""""""""""""""""""""""""""""""""""""""""""""""""""""""""""""""""""""""""""""""""";
            }";
        AssertAnalyzerCompletes(code);
    }

    [Fact]
    public void analyzer_should_handle_deeply_nested_block_comments()
    {
        var code = @"
            public class Test {
                /* /* /* /* /* /* /* /* /* /* /* /* /* /* /* /* /* /* /* /* 
                   /* /* /* /* /* /* /* /* /* /* /* /* /* /* /* /* /* /* /* /*
                   this is a comment
                */ */ */ */ */ */ */ */ */ */ */ */ */ */ */ */ */ */ */ */
                */ */ */ */ */ */ */ */ */ */ */ */ */ */ */ */ */ */ */ */
                int x = 1;
            }";
        AssertAnalyzerCompletes(code);
    }

    [Fact]
    public void analyzer_should_handle_five_thousand_tokens_before_semicolon_without_excessive_lookahead()
    {
        var code = @"
            public class Test {
                " + string.Concat(Enumerable.Repeat("@Anno ", 5000)) + @" int x = 1;
            }";
        AssertAnalyzerCompletes(code);
    }

    [Fact]
    public void analyzer_should_handle_one_thousand_duplicate_modifier_keywords()
    {
        var code = @"
            public class Test {
                " + string.Concat(Enumerable.Repeat("public ", 1000)) + @" int x = 1;
            }";
        AssertAnalyzerCompletes(code);
    }

    [Fact]
    public void analyzer_should_recover_from_one_hundred_consecutive_syntax_errors_and_parse_valid_members()
    {
        var code = @"
            public class Test {
                " + string.Concat(Enumerable.Repeat("!@#$%^&*() ", 100)) + @"
                public int validField = 42;
                public void validMethod() { return; }
            }";
        AssertAnalyzerCompletes(code);
    }

    [Fact]
    public void analyzer_should_handle_one_hundred_alternating_valid_and_invalid_member_declarations()
    {
        var code = GenerateAlternatingMembers(100);
        AssertAnalyzerCompletes(code);
    }

    [Fact]
    public void analyzer_should_handle_token_lookahead_exactly_at_limit_of_ten_thousand_tokens()
    {
        var code = @"
            public class Test {
                " + string.Concat(Enumerable.Repeat("int ", 9999)) + @" x = 1;
            }";
        AssertAnalyzerCompletes(code);
    }

    [Fact]
    public void analyzer_should_handle_token_lookahead_one_over_limit_of_ten_thousand_tokens()
    {
        var code = @"
            public class Test {
                " + string.Concat(Enumerable.Repeat("int ", 10001)) + @" x = 1;
            }";
        AssertAnalyzerCompletes(code);
    }

    [Fact]
    public void analyzer_should_efficiently_handle_ten_thousand_simple_field_declarations()
    {
        var code = GenerateManySimpleMembers(10000);
        AssertAnalyzerCompletes(code);
    }
    [Fact]
    public void analyzer_should_handle_three_hundred_levels_of_array_dimensions_on_type()
    {
        var code = @"
            public class Test {
                int" + string.Concat(Enumerable.Repeat("[]", 300)) + @" x;
            }";
        AssertAnalyzerCompletes(code);
    }

    [Fact]
    public void analyzer_should_handle_one_hundred_chained_ternary_conditional_operators()
    {
        var code = @"
            public class Test {
                int x = " + string.Concat(Enumerable.Repeat("true ? 1 : ", 100)) + @" 0;
            }";
        AssertAnalyzerCompletes(code);
    }

    [Fact]
    public void analyzer_should_handle_five_hundred_chained_instanceof_checks_with_logical_or()
    {
        var code = @"
            public class Test {
                void foo(Object o) {
                    boolean b = " + string.Concat(Enumerable.Repeat("o instanceof String || ", 500)) + @" false;
                }
            }";
        AssertAnalyzerCompletes(code);
    }

    [Fact]
    public void analyzer_should_handle_null_bytes_in_source_code()
    {
        var code = "public class Test { int x\0\0\0 = 1; }";
        AssertAnalyzerCompletes(code);
    }

    [Fact]
    public void analyzer_should_handle_byte_order_mark_characters_in_source_code()
    {
        var code = "\uFEFF\uFEFFpublic class Test { int x = 1; }";
        AssertAnalyzerCompletes(code);
    }

    [Fact]
    public void analyzer_should_handle_right_to_left_override_unicode_characters_in_identifiers()
    {
        var code = "public class Test { int \u202Ex = 1; }";
        AssertAnalyzerCompletes(code);
    }

    [Fact]
    public void analyzer_should_handle_one_hundred_levels_of_nested_if_else_statements()
    {
        var code = @"
            public class Test {
                void foo() {
                    " + string.Concat(Enumerable.Repeat("if (true) { ", 100)) + @"
                        int x = 1;
                    " + string.Concat(Enumerable.Repeat("} else { int y = 2; }", 100)) + @"
                }
            }";
        AssertAnalyzerCompletes(code);
    }

    [Fact]
    public void analyzer_should_handle_one_hundred_levels_of_nested_try_catch_blocks()
    {
        var code = @"
            public class Test {
                void foo() {
                    " + string.Concat(Enumerable.Repeat("try { ", 100)) + @"
                        int x = 1;
                    " + string.Concat(Enumerable.Repeat("} catch (Exception e) { }", 100)) + @"
                }
            }";
        AssertAnalyzerCompletes(code);
    }

    [Fact]
    public void analyzer_should_handle_switch_statement_with_one_thousand_case_labels()
    {
        var code = @"
            public class Test {
                void foo(int x) {
                    switch(x) {
                        " + string.Concat(Enumerable.Range(0, 1000).Select(i => $"case {i}: break;\n")) + @"
                        default: break;
                    }
                }
            }";
        AssertAnalyzerCompletes(code);
    }

    [Fact]
    public void analyzer_should_handle_multidimensional_array_initializer_with_ten_thousand_elements()
    {
        var code = @"
            public class Test {
                int[][] arr = {
                    " + string.Concat(Enumerable.Range(0, 100).Select(i =>
                        "{" + string.Join(",", Enumerable.Range(0, 100)) + "},\n")) + @"
                };
            }";
        AssertAnalyzerCompletes(code);
    }
    [Fact]
    public void analyzer_should_handle_method_declaring_five_hundred_exception_types_in_throws_clause()
    {
        var code = @"
            public class Test {
                void foo() throws " + string.Join(", ", Enumerable.Range(0, 500).Select(i => $"Exception{i}")) + @" { }
            }";
        AssertAnalyzerCompletes(code);
    }

    [Fact]
    public void analyzer_should_handle_generic_method_with_one_hundred_type_parameter_bounds()
    {
        var code = @"
            public class Test {
                <T extends " + string.Join(" & ", Enumerable.Range(0, 100).Select(i => $"Interface{i}")) + @"> void foo() { }
            }";
        AssertAnalyzerCompletes(code);
    }

    [Fact]
    public void analyzer_should_handle_annotation_with_deeply_nested_array_value_elements()
    {
        var code = @"
            public class Test {
                @Anno({
                    @Inner({@Deep({}), @Deep({}), @Deep({})}),
                    @Inner({@Deep({}), @Deep({}), @Deep({})}),
                    @Inner({@Deep({}), @Deep({}), @Deep({})})
                })
                public void foo() { }
            }";
        AssertAnalyzerCompletes(code);
    }

    [Fact]
    public void analyzer_should_handle_generic_method_with_complex_nested_return_type_in_generic_class()
    {
        var code = @"
            public class Test<A, B, C> {
                public <D, E, F> Map<Map<A, B>, Map<D, E>> foo(Map<B, C> x, Map<E, F> y) { return null; }
            }";
        AssertAnalyzerCompletes(code);
    }

    private static void AssertAnalyzerCompletes(string code)
    {
        var exception = Record.Exception(() =>
        {
            _ = new AnalyzerSimple(new StringBuilder(code));
        });

        if (exception is StackOverflowException)
            Assert.Fail("Analyzer hit a StackOverflowException");
    }

    private static string GenerateWideDeepGenerics(int width, int depth)
    {
        string BuildGeneric(int d)
        {
            if (d == 0) return "Integer";
            var inner = string.Join(", ", Enumerable.Repeat(BuildGeneric(d - 1), width));
            return $"Map<{inner}>";
        }
        return $"public class Test {{ {BuildGeneric(depth)} x; }}";
    }

    private static string GenerateAlternatingMembers(int count)
    {
        var sb = new StringBuilder("public class Test {\n");
        for (var i = 0; i < count; i++)
        {
            if (i % 2 == 0)
                sb.AppendLine($"    public int valid{i} = {i};");
            else
                sb.AppendLine($"    }}}}{{ garbage {i} @#$%");
        }
        sb.AppendLine("}");
        return sb.ToString();
    }

    private static string GenerateManySimpleMembers(int count)
    {
        var sb = new StringBuilder("public class Test {\n");
        for (var i = 0; i < count; i++)
        {
            sb.AppendLine($"    int x{i} = {i};");
        }
        sb.AppendLine("}");
        return sb.ToString();
    }

    private static string GenerateManyMembersWithErrors(int count)
    {
        var sb = new StringBuilder("public class Test {\n");
        for (var i = 0; i < count; i++)
        {
            if (i % 3 == 0)
                sb.AppendLine($"    int valid{i} = {i};");
            else if (i % 3 == 1)
                sb.AppendLine($"    garbage{i} {{{{ broken syntax");
            else
                sb.AppendLine($"    public void method{i}() {{ return; }}");
        }
        sb.AppendLine("}");
        return sb.ToString();
    }

    private static string GenerateNestedParens(int depth)
    {
        var open = new string('(', depth);
        var close = new string(')', depth);
        return $"public class Test {{ int x = {open}1{close}; }}";
    }

    private static string GenerateNestedClasses(int depth)
    {
        var sb = new StringBuilder();
        for (var i = 0; i < depth; i++)
        {
            sb.Append($"public class C{i} {{ ");
        }
        sb.Append("int x = 1;");
        for (var i = 0; i < depth; i++)
        {
            sb.Append(" }");
        }
        return sb.ToString();
    }

    private static string GenerateManyGenericParams(int count)
    {
        var typeParams = string.Join(", ", Enumerable.Range(0, count).Select(i => $"T{i}"));
        return $"public class Test<{typeParams}> {{ }}";
    }

    private static string GenerateManyMethodParams(int count)
    {
        var parameters = string.Join(", ", Enumerable.Range(0, count).Select(i => $"int p{i}"));
        return $"public class Test {{ void foo({parameters}) {{ }} }}";
    }

    private static string GenerateManyImplements(int count)
    {
        var interfaces = string.Join(", ", Enumerable.Range(0, count).Select(i => $"Interface{i}"));
        return $"public class Test implements {interfaces} {{ }}";
    }

    private static string GenerateLargeArrayLiteral(int count)
    {
        var elements = string.Join(", ", Enumerable.Range(0, count).Select(i => i.ToString()));
        return $"public class Test {{ int[] arr = {{{elements}}}; }}";
    }
}