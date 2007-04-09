XMLRPC_ASSEMBLY = XmlRpcCS.dll

XMLRPC_SRC = $(srcdir)/src/nwc/xmlrpc/*.cs

xmlrpcdir = $(pkglibdir)
xmlrpc_SCRIPTS = \
	$(XMLRPC_ASSEMBLY) 

all: $(XMLRPC_ASSEMBLY)

$(XMLRPC_ASSEMBLY):
	$(MCS) -target:library -out:$@ -unsafe $(XMLRPC_SRC)

EXTRA_DIST = $(XMLRPC_SRC)
DISTCLEANFILES = $(XMLRPC_ASSEMBLY) $(XMLRPC_ASSEMBLY).mdb
