serializationdir = $(pkglibdir)
serialization_SCRIPTS = Gendarme.Rules.Serialization.dll 
serialization_DATA = 
EXTRA_DIST = 
CLEANFILES = Gendarme.Rules.Serialization.dll Test.Rules.Serialization.dll *.mdb TestResult.xml 
DISTCLEANFILES = Makefile.in TestResult.xml

serialization_sources = \
	MarkAllNonSerializableFieldsRule.cs \
	CallBaseMethodsOnISerializableTypesRule.cs \
	ImplementISerializableCorrectlyRule.cs

serialization_test_sources = \
	MarkAllNonSerializableFieldsTest.cs \
	CallBaseMethodsOnISerializableTypesTest.cs \
	ImplementISerializableCorrectlyTest.cs

serialization_build_sources = $(addprefix $(srcdir)/, $(serialization_sources))
serialization_test_build_sources = $(addprefix $(srcdir)/Test/, $(serialization_test_sources))

Gendarme.Rules.Serialization.dll: $(serialization_build_sources) 
	$(GMCS) -target:library -debug -pkg:gendarme-framework -pkg:mono-cecil -out:$@ $(serialization_build_sources)

Test.Rules.Serialization.dll: $(serialization_test_build_sources) Gendarme.Rules.Serialization.dll
	$(GMCS) -target:library -debug -pkg:mono-nunit -r:Gendarme.Rules.Serialization.dll -r:Test.Rules.dll -out:$@ $(serialization_test_build_sources)

test: Test.Rules.Serialization.dll

run-test: test
	nunit-console2 Test.Rules.Serialization.dll

run-gendarme: Gendarme.Rules.Serialization.dll
	gendarme Gendarme.Rules.Serialization.dll
