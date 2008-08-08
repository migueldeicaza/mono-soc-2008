correctnessdir = $(pkglibdir)
correctness_SCRIPTS = Gendarme.Rules.Correctness.dll 
correctness_DATA = 
EXTRA_DIST = 
CLEANFILES = Gendarme.Rules.Correctness.dll Test.Rules.Correctness.dll *.mdb \
	TestResult.xml Test.Rules.Correctness.Good.Assembly.dll \
	Test.Rules.Correctness.Bad.Assembly.dll 
DISTCLEANFILES = Makefile.in TestResult.xml

correctness_sources = \
	ProvideCorrectArgumentsToFormattingMethodsRule.cs \
	AttributeStringLiteralsShouldParseCorrectlyRule.cs

correctness_test_sources = \
	ProvideCorrectArgumentsToFormattingMethodsTest.cs \
	AttributeStringLiteralsShouldParseCorrectlyTest.cs	

good_assembly_test_sources = \
	GoodAssemblyAttributeStringLiteralsShouldParseCorrectlyTest.cs

bad_assembly_test_sources = \
	BadAssemblyAttributeStringLiteralsShouldParseCorrectlyTest.cs

correctness_build_sources = $(addprefix $(srcdir)/, $(correctness_sources))
correctness_test_build_sources = $(addprefix $(srcdir)/Test/, $(correctness_test_sources))
good_assembly_test_build_sources = $(addprefix $(srcdir)/Test/, $(good_assembly_test_sources))
bad_assembly_test_build_sources = $(addprefix $(srcdir)/Test/, $(bad_assembly_test_sources))

Gendarme.Rules.Correctness.dll: $(correctness_build_sources) 
	$(GMCS) -target:library -debug -pkg:gendarme-framework -pkg:mono-cecil -out:$@ $(correctness_build_sources)

Test.Rules.Correctness.dll: $(correctness_test_build_sources) Gendarme.Rules.Correctness.dll
	$(GMCS) -target:library -debug -pkg:mono-nunit -r:Gendarme.Rules.Correctness.dll -r:Test.Rules.dll -out:$@ $(correctness_test_build_sources)

Test.Rules.Correctness.Good.Assembly.dll: $(good_assembly_test_build_sources) Test.Rules.Correctness.dll
	$(GMCS) -target:library -debug -pkg:mono-nunit -pkg:gendarme-framework -pkg:mono-cecil -r:Gendarme.Rules.Correctness.dll -r:Test.Rules.Correctness.dll -r:Test.Rules.dll -out:$@ $(good_assembly_test_build_sources)

Test.Rules.Correctness.Bad.Assembly.dll: $(bad_assembly_test_build_sources) Test.Rules.Correctness.dll
	$(GMCS) -target:library -debug -pkg:mono-nunit -pkg:gendarme-framework -pkg:mono-cecil -r:Gendarme.Rules.Correctness.dll -r:Test.Rules.Correctness.dll -r:Test.Rules.dll -out:$@ $(bad_assembly_test_build_sources)


test: Test.Rules.Correctness.dll Test.Rules.Correctness.Good.Assembly.dll Test.Rules.Correctness.Bad.Assembly.dll

run-test: test
	nunit-console2 Test.Rules.Correctness.dll 
	nunit-console2 Test.Rules.Correctness.Good.Assembly.dll
	nunit-console2 Test.Rules.Correctness.Bad.Assembly.dll

run-gendarme: Gendarme.Rules.Correctness.dll
	gendarme Gendarme.Rules.Correctness.dll
