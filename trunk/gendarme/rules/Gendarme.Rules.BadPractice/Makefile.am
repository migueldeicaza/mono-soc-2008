badpracticedir = $(pkglibdir)
badpractice_SCRIPTS = Gendarme.Rules.BadPractice.dll 
badpractice_DATA = 
EXTRA_DIST = 
CLEANFILES = Gendarme.Rules.BadPractice.dll Test.Rules.BadPractice.dll *.mdb TestResult.xml 
DISTCLEANFILES = Makefile.in TestResult.xml

badpractice_sources = \
	AvoidCallingProblematicMethodsRule.cs

badpractice_test_sources = \
	AvoidCallingProblematicMethodsTest.cs	

badpractice_build_sources = $(addprefix $(srcdir)/, $(badpractice_sources))
badpractice_test_build_sources = $(addprefix $(srcdir)/Test/, $(badpractice_test_sources))

Gendarme.Rules.BadPractice.dll: $(badpractice_build_sources) 
	$(GMCS) -target:library -debug -pkg:gendarme-framework -pkg:mono-cecil -out:$@ $(badpractice_build_sources)

Test.Rules.BadPractice.dll: $(badpractice_test_build_sources) Gendarme.Rules.BadPractice.dll
	$(GMCS) -target:library -debug -pkg:mono-nunit -r:Gendarme.Rules.BadPractice.dll -r:Test.Rules.dll -out:$@ $(badpractice_test_build_sources)

test: Test.Rules.BadPractice.dll

run-test: test
	nunit-console2 Test.Rules.BadPractice.dll

run-gendarme: Gendarme.Rules.BadPractice.dll
	gendarme Gendarme.Rules.BadPractice.dll
