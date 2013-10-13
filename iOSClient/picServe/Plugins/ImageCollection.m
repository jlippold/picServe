#import "ImageCollection.h"
#import "Base64.h"
#import "CVCell.h"
#import "CVHeader.h"
#import <QuartzCore/QuartzCore.h>
#import "SSZipArchive.h"

#import "ASIHTTPRequest.h"
#import "ASINetworkQueue.h"


@implementation ImageCollection;
@synthesize collectionView = _collectionView;
@synthesize searchBar = _searchBar;
@synthesize searchController = _searchController;
@synthesize managedObjectContext = _managedObjectContext;
@synthesize navBar = _navbar;
@synthesize networkQueue;


static dispatch_queue_t concurrentQueue = NULL;


-(CDVPlugin*) initWithWebView:(UIWebView*)theWebView
{
    self = (ImageCollection*)[super initWithWebView:theWebView];
    return self;
}


#pragma mark - JS interface methods

- (void)createImageView:(NSArray*)arguments withDict:(NSDictionary*)options
{

    //setup a network queue
    [self setNetworkQueue:[ASINetworkQueue queue]];
    [[self networkQueue] setDelegate:self];
    [[self networkQueue] setRequestDidFinishSelector:@selector(requestFinished:)];
    [[self networkQueue] setRequestDidFailSelector:@selector(requestFailed:)];
    [[self networkQueue] setQueueDidFinishSelector:@selector(queueFinished:)];
    [[self networkQueue] setShouldCancelAllRequestsOnFailure:NO];
    [self networkQueue].maxConcurrentOperationCount = 2;
    
    
    CGRect navBarFrame = CGRectMake(0, 0, self.webView.superview.bounds.size.width, 44.0);
    _navbar = [[UINavigationBar alloc] initWithFrame:navBarFrame];
    
    _navbar.barStyle = UIBarStyleBlack;
    UIImage *backgroundImage = [UIImage imageNamed:@"www/img/navBar.png"];
    [_navbar setBackgroundImage:backgroundImage forBarMetrics:0];
    
    UINavigationItem *navItem = [UINavigationItem alloc];
    NSString *navTitle = [options objectForKey:@"navTitle"];
    navItem.title = navTitle;
    
    [_navbar pushNavigationItem:navItem animated:false];
    [self.webView.superview addSubview:_navbar];
    [_navbar setHidden:YES];
    
    if ( [[options objectForKey:@"showBackButton"] boolValue] == true) {

        UIButton *settingsView = [[UIButton alloc] initWithFrame:CGRectMake(0.0, 100.0, 60.0, 30.0)];
        [settingsView addTarget:self action:@selector(onBackButtonPress:) forControlEvents:UIControlEventTouchUpInside];
        UIImage *backButtonImage = [[UIImage imageNamed:@"www/img/UINavigationBarBlackOpaqueBack.png"] resizableImageWithCapInsets:UIEdgeInsetsMake(0, 13, 0, 6)];

        [settingsView setBackgroundImage:backButtonImage forState:UIControlStateNormal];
        [settingsView setTitle:[options objectForKey:@"backButtonText"] forState:UIControlStateNormal];
        settingsView.titleLabel.font = [UIFont boldSystemFontOfSize:15.0f];
        settingsView.titleLabel.adjustsFontSizeToFitWidth = TRUE;
        [settingsView.layer setMasksToBounds:YES];
        [settingsView setTitleEdgeInsets:UIEdgeInsetsMake(0.0, 10.0, 0.0, 5.0)];

        
        UIBarButtonItem *settingsButton = [[UIBarButtonItem alloc] initWithCustomView:settingsView];
        [navItem setLeftBarButtonItem:settingsButton];
        
    }
    
    if ( [[options objectForKey:@"showRightButton"] boolValue] == true) {
        UIBarButtonItem *rightButton = [[UIBarButtonItem alloc] initWithBarButtonSystemItem:UIBarButtonSystemItemAction target:self action:@selector(onRightButtonPress:)];
         navItem.rightBarButtonItem = rightButton;
    } else {
        UIBarButtonItem *rightButtonEdit = [[UIBarButtonItem alloc] initWithTitle:@"Edit" style:UIBarButtonSystemItemDone target:self action:@selector(onEdit:)];
        navItem.rightBarButtonItem = rightButtonEdit;
    }
    
    

    //Download zip file
    if ([[options objectForKey:@"zipcache"] isEqualToString:@""] ) {
        
    } else {
        //NSLog(@"ZIP!");
        dispatch_queue_t queue = dispatch_get_global_queue(0,0);
        dispatch_async(queue, ^{
            NSArray *paths = NSSearchPathForDirectoriesInDomains(NSDocumentDirectory,  NSUserDomainMask, YES);
            NSString *documentsDirectory = [paths objectAtIndex:0];
            NSString *cachedFile = [documentsDirectory stringByAppendingPathComponent:[NSString stringWithFormat:@"%@.zip", [options objectForKey:@"zipcache"] ]];
            
            NSString *url = [NSString stringWithFormat:@"%@&Path=%@", [options objectForKey:@"zipAddress"], [options objectForKey:@"zipcache"]];
            NSData *zipData = [NSData  dataWithContentsOfURL:[NSURL URLWithString:url]];
            [zipData writeToURL:[NSURL fileURLWithPath:cachedFile] options:0 error:nil];
            if ( [[NSFileManager defaultManager] fileExistsAtPath:cachedFile] ) {
                [SSZipArchive unzipFileAtPath:cachedFile toDestination:[paths objectAtIndex:0]];
                [[NSFileManager defaultManager] removeItemAtPath:cachedFile error:nil];
                //NSLog(@"unZIPPED!");
            }
        });
    }

    
    _mainTableHeight = 200;
    
    UICollectionViewFlowLayout *flowLayout = [[UICollectionViewFlowLayout alloc] init];
    [flowLayout setScrollDirection:UICollectionViewScrollDirectionVertical];
    [flowLayout setItemSize:CGSizeMake(100, 100)];
    [flowLayout setMinimumInteritemSpacing:0.f];
    [flowLayout setMinimumLineSpacing:5];
    [flowLayout setSectionInset:UIEdgeInsetsMake(5, 5, 5, 5)];
    flowLayout.headerReferenceSize = CGSizeMake(0, 30);
    _originalWebViewFrame = self.webView.frame;
	
	CGRect myFrame;
	
	myFrame = CGRectMake(
                                _originalWebViewFrame.origin.x,
                                _originalWebViewFrame.origin.y + 44.0,
                                _originalWebViewFrame.size.width,
                                _originalWebViewFrame.size.height - 44.0
                                );
    
    self.webView.scrollView.scrollsToTop = NO;
    _collectionView.scrollsToTop = YES;

    _collectionView = [[UICollectionView alloc] initWithFrame:myFrame collectionViewLayout:flowLayout];

    _collectionView.backgroundColor = [UIColor scrollViewTexturedBackgroundColor];
    [_collectionView setDataSource:self];
    [_collectionView setDelegate:self];
    [_collectionView setBounces:YES];
    [_collectionView registerClass:[CVCell class] forCellWithReuseIdentifier:@"cvCell"];
    [_collectionView setHidden:YES];
    
 
   [_collectionView registerClass:[HeaderView class] forSupplementaryViewOfKind:UICollectionElementKindSectionHeader withReuseIdentifier:@"HeaderView"];
    
    isEditing = NO;
    

	self.webView.superview.autoresizesSubviews = YES;
	[self.webView.superview addSubview:_collectionView];
    
}

- (void)setImageViewData:(NSArray*)arguments withDict:(NSDictionary*)options
{
	_imageViewData = [[arguments objectAtIndex:0] mutableCopy];
	[_collectionView reloadData];
	
}


- (UICollectionViewCell *)collectionView:(UICollectionView *)cv cellForItemAtIndexPath:(NSIndexPath *)indexPath {

    /* pull from data source */
	int section = indexPath.section;
    int row = indexPath.row;
    
    NSMutableArray *tmp = [[NSMutableArray alloc] init];
    if (isFiltered) {
        tmp = _searchResults.copy;
    } else {
        tmp = _imageViewData.copy;
    }
    
    int counter = 0;
    int rowCounter = 0;
    int sectionRowCounter = 0;
    int actualRow = 0;
    NSString *loopedHeader = @"";
    BOOL isCurrentSection = false;
    
    for(int i = 0; i < [tmp count]; i++ ) {
        NSString *thisHeader = [[tmp objectAtIndex:i] valueForKey:@"sectionHeader"];
        if ( ![thisHeader isEqualToString:loopedHeader]) {
            loopedHeader = thisHeader;
            isCurrentSection = false;
            if (counter == section) {
                isCurrentSection = true;
            }
            counter++;
        }
        
        if (isCurrentSection) {
            if (sectionRowCounter == row ) {
                actualRow = rowCounter;
            }
            sectionRowCounter++;
        }
        rowCounter++;
    }

    
    NSDictionary *item = [_imageViewData objectAtIndex:actualRow];
    if (isFiltered) {
        item = [_searchResults objectAtIndex:actualRow];
    }
    
    /* end pull from data source */
    
    CVCell *cell = (CVCell *)[cv dequeueReusableCellWithReuseIdentifier:@"cvCell" forIndexPath:indexPath];
    if (cell == nil) {
        cell = [[CVCell alloc] init];
    }
    cell.tag = actualRow;
    
    
    NSString *url = [item valueForKey:@"image"];
    
    NSString *cachePath = [item valueForKey:@"cachePath"];

    cell.imageView.image = [UIImage imageNamed:@"www/img/photo.png"];
    
    if ([cachePath hasSuffix:@".mov"]) {
        [cell.videoOverlay setHidden:NO];
    } else {
        [cell.videoOverlay setHidden:YES];
    }
    
    if (![url hasPrefix:@"http"]) {
        [cell.imageView.layer setBorderColor: [[UIColor clearColor] CGColor]];
        [cell.imageView.layer setBorderWidth: 0.0];
        cell.imageView.image = [UIImage imageNamed:[item valueForKey:@"image"]];
        [cell.deleteButton setHidden:YES];
    } else {
        [cell.imageView.layer setBorderColor: [[UIColor whiteColor] CGColor]];
        [cell.imageView.layer setBorderWidth: 2.0];
        if (isEditing) {
            [cell.deleteButton setHidden:NO];
        }else{
            [cell.deleteButton setHidden:YES];
        }
        
        //check if the cached version exists
        NSArray *paths = NSSearchPathForDirectoriesInDomains(NSDocumentDirectory,  NSUserDomainMask, YES);
        NSString *documentsDirectory = [paths objectAtIndex:0];
        NSString *cachedFile = [documentsDirectory stringByAppendingPathComponent:[NSString stringWithFormat:@"%@.cache", cachePath ]];
        
        BOOL fileExists = [[NSFileManager defaultManager] fileExistsAtPath:cachedFile];
        if (fileExists) {
            NSString *content = [NSString stringWithContentsOfFile:cachedFile encoding:NSUTF8StringEncoding error:nil];
            NSData *imageData = [NSData dataWithContentsOfURL:[NSURL URLWithString:content]];
            cell.imageView.image = [UIImage imageWithData:imageData];
            [cell setNeedsLayout];
        } else {
            BOOL needsKickStart = NO;
            if ([[self networkQueue] requestsCount] == 0) {
                needsKickStart = YES;
            }

            ASIHTTPRequest *request = [ASIHTTPRequest requestWithURL:[[NSURL alloc] initWithString:url]];
            NSDictionary *reqInfo = [NSDictionary dictionaryWithObjectsAndKeys:
                                     [NSString stringWithFormat:@"%d",row], @"row",
                                     [NSString stringWithFormat:@"%d",section], @"section",
                                    cachedFile, @"cachedFile",
                                     nil];
            
            [request setUserInfo:reqInfo];
            request.tag = actualRow;
            [[self networkQueue] addOperation:request];
  
            if (needsKickStart) {
                [[self networkQueue] go];
            }
            
            
            /*
            dispatch_queue_t concurrentQueue = dispatch_get_global_queue(DISPATCH_QUEUE_PRIORITY_HIGH,  0ul);
            dispatch_async(concurrentQueue, ^{
                NSData *image = [[NSData alloc] initWithContentsOfURL:[[NSURL alloc] initWithString:url]];
                NSData* data = UIImagePNGRepresentation([UIImage imageWithData:image]);
                NSString *strEncoded = [Base64 encode:data];
                if (![strEncoded isEqualToString:@""]) {
                    strEncoded = [@"data:image/jpg;base64," stringByAppendingString:strEncoded];
                    [strEncoded writeToFile:cachedFile atomically:YES encoding:NSUTF8StringEncoding error:nil];
                    NSData *imageData = [NSData dataWithContentsOfURL:[NSURL URLWithString:strEncoded]];
                    
                    dispatch_async(dispatch_get_main_queue(),^{
                        if (indexPath.row > 0 && indexPath.section > 0) {
                            CVCell *cell1 = (CVCell *)[cv cellForItemAtIndexPath:indexPath];
                            if (cell1) {
                                cell.imageView.image = [UIImage imageWithData:imageData];
                                [cell setNeedsLayout];
                                [_collectionView reloadItemsAtIndexPaths:@[indexPath]];
                            }
                        }
                    });
                    
                }
            });
             */

        }
        
    }

    [cell.titleLabel setText:[item valueForKey:@"name"]];
    
    cell.titleLabel.shadowColor = [UIColor blackColor];
    cell.titleLabel.shadowOffset = CGSizeMake(0,-0.4);
    

    [cell.deleteButton addTarget:self action:@selector(onDelete:event:) forControlEvents:UIControlEventTouchUpInside];
    
    return cell;
}

- (void)collectionView:(UICollectionView *)collectionView didSelectItemAtIndexPath:(NSIndexPath *)indexPath {
    //NSLog(@"do");
    
    if (isEditing) {
        return;
    }
    
	int section = indexPath.section;
    int row = indexPath.row;
    
    NSMutableArray *tmp = [[NSMutableArray alloc] init];
    if (isFiltered) {
        tmp = _searchResults.copy;
    } else {
        tmp = _imageViewData.copy;
    }
    
    int counter = 0;
    int rowCounter = 0;
    int sectionRowCounter = 0;
    int actualRow = 0;
    NSString *loopedHeader = @"";
    BOOL isCurrentSection = false;
    
    for(int i = 0; i < [tmp count]; i++ ) {
        NSString *thisHeader = [[tmp objectAtIndex:i] valueForKey:@"sectionHeader"];
        if ( ![thisHeader isEqualToString:loopedHeader]) {
            loopedHeader = thisHeader;
            isCurrentSection = false;
            if (counter == section) {
                isCurrentSection = true;
            }
            counter++;
        }
        
        if (isCurrentSection) {
            if (sectionRowCounter == row ) {
                actualRow = rowCounter;
                if (isFiltered) {
                    //we have the searched item, but we need to pull the original index
                    actualRow = [[[tmp objectAtIndex:i] objectForKey:@"index"] intValue];
                }
            }
            sectionRowCounter++;
        }
        rowCounter++;
    }
    
    NSString * jsCallBack = [NSString stringWithFormat:@"window.plugins.ImageCollection._onImageViewRowSelect(%d);", actualRow];
    [self.webView stringByEvaluatingJavaScriptFromString:jsCallBack];
}

- (UICollectionReusableView *)collectionView:(UICollectionView *)collectionView viewForSupplementaryElementOfKind:(NSString *)kind atIndexPath:(NSIndexPath *)indexPath
{

    HeaderView *header = [collectionView dequeueReusableSupplementaryViewOfKind:UICollectionElementKindSectionHeader withReuseIdentifier:@"HeaderView" forIndexPath:indexPath];
    
    UILabel *myLabel = (UILabel *)[header viewWithTag:1];
    if (!myLabel) {
        myLabel = [[UILabel alloc]initWithFrame:CGRectMake(0, 0, 320, 26)];
        myLabel.tag = 1;
        [myLabel setBackgroundColor:[[UIColor blackColor] colorWithAlphaComponent:0.5f]];
        [myLabel setFont:[UIFont boldSystemFontOfSize:18.0f]];
        myLabel.textColor = [UIColor whiteColor];
        myLabel.shadowColor = [UIColor blackColor];
        myLabel.shadowOffset = CGSizeMake(0,1);
        myLabel.opaque = YES;
        [myLabel setOpaque:YES];
    }
    
    NSMutableArray *tmp = [[NSMutableArray alloc] init];
    if (isFiltered) {
        tmp = _searchResults.copy;
    } else {
        tmp = _imageViewData.copy;
    }
    int counter = -1;
    NSString *loopedHeader = @"";
    NSString *retVal = @"";
    
    for(int i = 0; i < [tmp count]; i++ ) {
        NSString *thisHeader = [[tmp objectAtIndex:i] valueForKey:@"sectionHeader"];
        if ( ![thisHeader isEqualToString:loopedHeader]) {
            loopedHeader = thisHeader;
            counter++;
        }
        if (indexPath.section == counter) {
            retVal = thisHeader;
        }
    }
    
    [myLabel setText:[NSString stringWithFormat:@"    %@", retVal]];
    [header addSubview:myLabel];
    return header;
}

- (NSInteger)numberOfSectionsInCollectionView: (UICollectionView *)collectionView {
    NSMutableArray *tmp = [[NSMutableArray alloc] init];
    if (isFiltered) {
        tmp = _searchResults.copy;
    } else {
        tmp = _imageViewData.copy;
    }
    int counter = 1;
    
    if ([tmp count] == 0) {
        return 0;
    }
    NSString *loopedHeader = [[tmp objectAtIndex:0] valueForKey:@"sectionHeader"];
    for(int i = 0; i < [tmp count]; i++ ) {
        NSString *thisHeader = [[tmp objectAtIndex:i] valueForKey:@"sectionHeader"];
        if ( ![thisHeader isEqualToString:loopedHeader]) {
            loopedHeader = thisHeader;
            counter++;
        }
    }
    //NSLog(@"Total Sections: %d", counter);
    return counter;
}

- (NSInteger)collectionView:(UICollectionView *)view numberOfItemsInSection:(NSInteger)section {
    
    NSMutableArray *tmp = [[NSMutableArray alloc] init];
    if (isFiltered) {
        tmp = _searchResults.copy;
    } else {
        tmp = _imageViewData.copy;
    }
    int counter = 0;
    int rowsInSection = 0;
    NSString *loopedHeader = @"";
    BOOL isCurrentSection = false;
    
    for(int i = 0; i < [tmp count]; i++ ) {
        NSString *thisHeader = [[tmp objectAtIndex:i] valueForKey:@"sectionHeader"];
        if ( ![thisHeader isEqualToString:loopedHeader]) {
            loopedHeader = thisHeader;
            isCurrentSection = false;
            if (counter == section) {
                isCurrentSection = true;
            }
            counter++;
        }
        
        if (isCurrentSection) {
            rowsInSection++;
        }
    }
    //NSLog(@"Section: %d Count: %d", section, rowsInSection);
    return rowsInSection;
}


- (void)showImageView:(NSArray*)arguments withDict:(NSDictionary*)optionsoptions
{
	if(nil == _collectionView){
        [self createImageView:nil withDict:nil];
	}
	
	if(NO == [_collectionView isHidden]){
		return;
	}
    
	
	_originalWebViewFrame = self.webView.frame;
	
	CGRect mainTableFrame, CDWebViewFrame;
	
	CDWebViewFrame = CGRectMake(
                                _originalWebViewFrame.origin.x,
                                _originalWebViewFrame.origin.y,
                                _originalWebViewFrame.size.width,
                                _originalWebViewFrame.size.height - _mainTableHeight
                                );
	
	mainTableFrame = CGRectMake(
                                _originalWebViewFrame.origin.x,
                                _originalWebViewFrame.origin.y + 44.0,
                                _originalWebViewFrame.size.width,
                                _originalWebViewFrame.size.height - 44.0
                                );
	
	[_collectionView setFrame:mainTableFrame];
	[_collectionView setHidden:NO];
    [_navbar setHidden:NO];
    [self fadeIn];

    
}
- (void)hideImageView:(NSArray *)arguments withDict:(NSDictionary *)options
{
    //    [self searchBarCancelButtonClicked:_searchBar];
    [[self networkQueue] cancelAllOperations];
    
	if(nil == _collectionView){
        return;
	}
	
	if(YES == [_collectionView isHidden]){
		return;
	}
    
    if (concurrentQueue) {
        //dispatch_release(concurrentQueue);
    }
    
    [_searchBar resignFirstResponder];
    
    
    [self fadeOut];
	
	[self.webView setFrame:_originalWebViewFrame];
    
	
}


- (IBAction)onBackButtonPress:(id)sender
{
    NSString * jsCallBack = @"window.plugins.ImageCollection._onBackButtonTap();";
    [self.webView stringByEvaluatingJavaScriptFromString:jsCallBack];
}

- (IBAction)onRightButtonPress:(id)sender
{
    NSString * jsCallBack = @"window.plugins.ImageCollection._onRightButtonTap();";
    [self.webView stringByEvaluatingJavaScriptFromString:jsCallBack];
}

- (IBAction)onEdit:(id)sender
{
    if (isEditing) {
        _navbar.topItem.rightBarButtonItem.title = @"Edit";
        _navbar.topItem.rightBarButtonItem.style = UIBarButtonSystemItemDone;
        isEditing = NO;
    } else {
        _navbar.topItem.rightBarButtonItem.title = @"Done";
        _navbar.topItem.rightBarButtonItem.style = UIBarButtonSystemItemEdit;
        isEditing = YES;
    }

    [_collectionView reloadData];
}

- (IBAction)doEditMode:(id)sender
{

    _navbar.topItem.rightBarButtonItem.title = @"Done";
    _navbar.topItem.rightBarButtonItem.style = UIBarButtonSystemItemEdit;
    isEditing = YES;

    [_collectionView reloadData];
    [self startQuivering];
}


- (IBAction)onDelete:(id)sender event:(id)event {
    NSSet *touches = [event allTouches];
    UITouch *touch = [touches anyObject];
    CGPoint currentTouchPosition = [touch locationInView:_collectionView];
    NSIndexPath *p = [_collectionView indexPathForItemAtPoint: currentTouchPosition];
    int row = p.row;
	int section = p.section;
    
    NSMutableArray *tmp = [[NSMutableArray alloc] init];
    if (isFiltered) {
        tmp = _searchResults.copy;
    } else {
        tmp = _imageViewData.copy;
    }
    
    int counter = 0;
    int rowCounter = 0;
    int sectionRowCounter = 0;
    int actualRow = 0;
    NSString *loopedHeader = @"";
    BOOL isCurrentSection = false;
    
    for(int i = 0; i < [tmp count]; i++ ) {
        NSString *thisHeader = [[tmp objectAtIndex:i] valueForKey:@"sectionHeader"];
        if ( ![thisHeader isEqualToString:loopedHeader]) {
            loopedHeader = thisHeader;
            isCurrentSection = false;
            if (counter == section) {
                isCurrentSection = true;
            }
            counter++;
        }
        
        if (isCurrentSection) {
            if (sectionRowCounter == row ) {
                actualRow = rowCounter;
                if (isFiltered) {
                    //we have the searched item, but we need to pull the original index
                    actualRow = [[[tmp objectAtIndex:i] objectForKey:@"index"] intValue];
                }
            }
            sectionRowCounter++;
        }
        rowCounter++;
    }
    
    
    UIAlertView *alert = [[UIAlertView alloc] initWithTitle:@"picServe" message:@"Are you sure you want to delete?" delegate:self cancelButtonTitle:@"Delete" otherButtonTitles:@"Cancel",nil];
    [alert setAlertViewStyle:UIAlertViewStyleDefault];
    alert.tag = actualRow;
    [alert show];
    

}


- (void)alertView:(UIAlertView *)alertView clickedButtonAtIndex:(NSInteger)buttonIndex
{
    if (buttonIndex == 0 ) {
        NSString * jsCallBack = [NSString stringWithFormat:@"window.plugins.ImageCollection._onRightButtonTap(%d);", alertView.tag];
        [self.webView stringByEvaluatingJavaScriptFromString:jsCallBack];
        [_imageViewData removeObjectAtIndex:alertView.tag];
        [_collectionView reloadData];
    }
}

-(void)fadeIn
{
    CGRect r = [_collectionView frame];
    r.origin.y = [_collectionView frame].size.height;
    [_collectionView setFrame:r];
    _collectionView.alpha = 0;
    _navbar.alpha = 0;
    [_navbar setHidden:NO];
    [_collectionView setHidden:NO];
    
    [UIView animateWithDuration:0.3
                          delay: 0.0
                        options:UIViewAnimationCurveEaseInOut
                     animations:^{
                         CGRect r = [_collectionView frame];
                         r.origin.y = 44.0;
                         [_collectionView setFrame:r];
                         _collectionView.alpha = 1;
                         _navbar.alpha =1 ;
                     }
                     completion:^(BOOL finished){
                         [self.webView stringByEvaluatingJavaScriptFromString:@"window.plugins.ImageCollection._onTableShowComplete();"];
                     }];
    
}

-(void)fadeOut
{
    
    
    CGRect r = [_collectionView frame];
    r.origin.y = _offsetTop;
    [_collectionView setFrame:r];
    _collectionView.alpha = 1;
    _navbar.alpha = 1;
    [_navbar setHidden:NO];
    [_collectionView setHidden:NO];
    
    [UIView animateWithDuration:0.3
                          delay: 0.0
                        options:UIViewAnimationCurveEaseInOut
                     animations:^{
                         CGRect r = [_collectionView frame];
                         r.origin.y = [_collectionView frame].size.height;
                         [_collectionView setFrame:r];
                         _collectionView.alpha = 0;
                         _navbar.alpha = 0;
                     }
                     completion:^(BOOL finished){
                    
                         [_navbar setHidden:YES];
                         [_collectionView setHidden:YES];
                         [self.webView stringByEvaluatingJavaScriptFromString:@"window.plugins.ImageCollection._onTableHideComplete();"];
                         
                     }];
    
    
}

- (void)startQuivering
{

    
}
- (void)stopQuivering
{
    CALayer *layer = _collectionView.layer;
    [layer removeAnimationForKey:@"quivering"];
}


- (void)requestFinished:(ASIHTTPRequest *)request
{
    NSString *row = [[request userInfo] valueForKey:@"row"] ;
    NSString *section = [[request userInfo] valueForKey:@"section"];
    NSString *cachedFile = [[request userInfo] valueForKey:@"cachedFile"];
    NSData *data = UIImagePNGRepresentation([UIImage imageWithData:[request responseData]]);
    NSString *strEncoded = [Base64 encode:data];

    if (![strEncoded isEqualToString:@""]) {
        strEncoded = [@"data:image/jpg;base64," stringByAppendingString:strEncoded];
        [strEncoded writeToFile:cachedFile atomically:YES encoding:NSUTF8StringEncoding error:nil];
        NSData *imageData = [NSData dataWithContentsOfURL:[NSURL URLWithString:strEncoded]];
        NSIndexPath *IP = [NSIndexPath indexPathForRow:[row intValue] inSection:[section intValue]];
        CVCell *cell1 = (CVCell *)[_collectionView cellForItemAtIndexPath:IP];
        if (cell1) {
            cell1.imageView.image = [UIImage imageWithData:imageData];
            [cell1 setNeedsLayout];
            [_collectionView reloadItemsAtIndexPaths:@[IP]];
        }
    }
}

- (void)requestFailed:(ASIHTTPRequest *)request
{

    //... Handle failure
    //set image to whatever instead..?
    //NSLog(@"Request failed");
}


- (void)queueFinished:(ASINetworkQueue *)queue
{
    //NSLog(@"Queue finished");
}

@end


