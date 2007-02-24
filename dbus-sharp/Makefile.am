DBUS_ASSEMBLY = NDesk.DBus.dll
DBUS_GLIB_ASSEMBLY = NDesk.DBus.GLib.dll

DBUS_SRC = $(srcdir)/dbus-sharp/*.cs
DBUS_GLIB_SRC = $(srcdir)/dbus-sharp-glib/*.cs

if !EXTERNAL_DBUS
dbusdir = $(pkglibdir)
dbus_SCRIPTS = \
	$(DBUS_ASSEMBLY) \
	$(DBUS_GLIB_ASSEMBLY) \
	$(DBUS_GLIB_ASSEMBLY).config

all: $(DBUS_ASSEMBLY) $(DBUS_GLIB_ASSEMBLY)

$(DBUS_ASSEMBLY): $(DBUS_SRC)
	$(MCS) -target:library -out:$@ -unsafe -r:Mono.Posix $(DBUS_SRC)

$(DBUS_GLIB_ASSEMBLY): $(DBUS_ASSEMBLY) $(DBUS_GLIB_SRC)
	$(MCS) -target:library -out:$@ -r:$(DBUS_ASSEMBLY) $(DBUS_GLIB_SRC)

endif

EXTRA_DIST = $(DBUS_SRC) $(DBUS_GLIB_SRC) $(DBUS_GLIB_ASSEMBLY).config.in
DISTCLEANFILES = $(DBUS_ASSEMBLY) $(DBUS_ASSEMBLY).mdb $(DBUS_GLIB_ASSEMBLY) $(DBUS_GLIB_ASSEMBLY).mdb
MAINTAINERCLEANFILES = Makefile.in

