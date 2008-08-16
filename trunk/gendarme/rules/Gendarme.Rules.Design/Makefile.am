designdir = $(pkglibdir)
design_SCRIPTS = Gendarme.Rules.Design.dll 
design_DATA = 
EXTRA_DIST = 
CLEANFILES = Gendarme.Rules.Design.dll Test.Rules.Design.dll *.mdb TestResult.xml
DISTCLEANFILES = Makefile.in TestResult.xml

design_sources = \
	DeclareEventHandlersCorrectlyRule.cs 

design_test_sources = \
	DeclareEventHandlersCorrectlyTest.cs 

design_build_sources = $(addprefix $(srcdir)/, $(design_sources))
design_test_build_sources = $(addprefix $(srcdir)/Test/, $(design_test_sources))

Gendarme.Rules.Design.dll: $(design_build_sources) 
	$(GMCS) -target:library -debug -pkg:gendarme-framework -pkg:mono-cecil -out:$@ $(design_build_sources)

Test.Rules.Design.dll: $(design_test_build_sources) Gendarme.Rules.Design.dll
	$(GMCS) -target:library -debug -pkg:mono-nunit -pkg:mono-cecil -r:Gendarme.Rules.Design.dll -r:Test.Rules.dll -out:$@ $(design_test_build_sources)

test: Test.Rules.Design.dll

run-test: test
	nunit-console2 Test.Rules.Design.dll 

run-gendarme: Gendarme.Rules.Design.dll
	gendarme Gendarme.Rules.Design.dll
