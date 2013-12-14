#import <Cordova/CDVPlugin.h>
#import <UIKit/UIKit.h>
#import <CoreData/CoreData.h>
#import "ASIHTTPRequest.h"
#import "ASINetworkQueue.h"

@interface ImageCollection : CDVPlugin <UICollectionViewDelegate,UICollectionViewDataSource>{

	CGRect _originalWebViewFrame;
    NSMutableArray* _data;
	NSString* _mainTableTitle;
	CGFloat _mainTableHeight;
    CGFloat _offsetTop;
    NSMutableArray* _searchResults;
	BOOL *isFiltered;
    BOOL *isEditing;
}

@property (nonatomic, strong) UICollectionView *collectionView;
@property (nonatomic, strong) UISearchBar *searchBar;
@property (nonatomic, strong) UINavigationBar *navBar;
@property (nonatomic, strong) ASINetworkQueue* networkQueue;

@property (nonatomic, strong) UISearchDisplayController *searchController;
@property (readonly, strong, nonatomic) NSManagedObjectContext *managedObjectContext;

- (void)createImageView:(NSArray*)arguments withDict:(NSDictionary*)options;
- (void)showImageView:(NSArray*)arguments withDict:(NSDictionary*)options;
- (void)hideImageView:(NSArray*)arguments withDict:(NSDictionary*)options;
- (void)setImageViewData:(NSArray*)arguments withDict:(NSDictionary*)options;

@end



