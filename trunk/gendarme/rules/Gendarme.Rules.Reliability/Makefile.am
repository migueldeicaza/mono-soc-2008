reliabilitydir = $(pkglibdir)
reliability_SCRIPTS = Gendarme.Rules.Reliability.dll 
reliability_DATA = 
EXTRA_DIST = 
CLEANFILES = Gendarme.Rules.dll Test.Reliability.dll *.mdb TestResult.xml 
DISTCLEANFILES = Makefile.in TestResult.xml

reliability_sources = \
	AvoidCallingProblematicMethodsRule.cs

reliability_test_sources = \
	AvoidCallingProblematicMethodsTest.cs	

reliability_build_sources = $(addprefix $(srcdir)/, $(reliability_sources))
reliability_test_build_sources = $(addprefix $(srcdir)/Test/, $(reliability_test_sources))

Gendarme.Rules.Reliability.dll: $(reliability_build_sources) 
	$(GMCS) -target:library -debug -r:System.Xml.Linq -pkg:gendarme-framework -pkg:mono-cecil -out:$@ $(reliability_build_sources)

Test.Rules.Reliability.dll: $(reliability_test_build_sources) Gendarme.Rules.Reliability.dll
	$(GMCS) -target:library -debug -pkg:mono-nunit -r:Gendarme.Rules.Reliability.dll -out:$@ $(reliability_test_build_sources)

test: Test.Rules.Reliability.dll

run-test: test
	nunit-console2 /xml=TestResult Test.Reliability.dll

run-gendarme: Gendarme.Rules.Reliability.dll
	gendarme Gendarme.Rules.Reliability.dll
