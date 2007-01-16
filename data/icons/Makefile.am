noinst_DATA =					\
	face-sad.png				\
	face-smile.png				\
	favourites-image.png			\
	f-spot-favorite.png			\
	f-spot-other.png			\
	people-image.png			\
	person-image.png			\
	secure.png				\
	unknown-cover.png		
	icon-theme-installer

theme=hicolor
themedir=$(datadir)/icons/$(theme)
theme_icons = \
	apps,last-exit-16.png \
	apps,last-exit-22.png \
	apps,last-exit-24.png \
	apps,last-exit.svg \
	actions,show-info-48.svg \
	actions,tag-new-48.svg \
	actions,show-info-32.png \
	actions,tag-new-32.png \
	actions,show-info-22.png \
	actions,tag-new-16.png \
	actions,tag-new-22.png

install_icon_exec = $(top_srcdir)/data/icons/icon-theme-installer \
	-t "$(theme)" \
	-s "$(srcdir)" \
	-d "x$(DESTDIR)" \
	-b "$(themedir)" \
	-m "$(mkinstalldirs)" \
	-x "$(INSTALL_DATA)"

MAINTAINERCLEANFILES = Makefile.in
EXTRA_DIST = \
	$(noinst_DATA) \
	icon-theme-installer

#	$(theme_icons) \
#
install-data-local:
	@-$(install_icon_exec) -i $(theme_icons)

uninstall-hook:
	@-$(install_icon_exec) -u $(theme_icons)
